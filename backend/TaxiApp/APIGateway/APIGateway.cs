using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Net.WebSockets;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Data;
using Common.WebSocketManager;
using System.Text;
using System.Net;
using Common.Models;
using System.Security.Claims;
using System.Security.Claims;
using Common.Interface;
using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json;
using Common.DTO;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace APIGateway
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance.
    /// </summary>
    internal sealed class APIGateway : StatelessService
    {
        private static readonly Common.WebSocketManager.WebSocketManager _webSocketManager = new Common.WebSocketManager.WebSocketManager();

        public APIGateway(StatelessServiceContext context)
            : base(context)
        { }
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[]
            {
                new ServiceInstanceListener(serviceContext =>
                    new KestrelCommunicationListener(serviceContext, "ServiceEndpoint", (url, listener) =>
                    {
                        ServiceEventSource.Current.ServiceMessage(serviceContext, $"Starting Kestrel on {url}");

                        var builder = WebApplication.CreateBuilder();


                        //JWT
                        var jwtIssuer = builder.Configuration.GetSection("Jwt:Issuer").Get<string>();
                        var jwtKey = builder.Configuration.GetSection("Jwt:Key").Get<string>();

                        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                         .AddJwtBearer(options =>
                         {
                             options.TokenValidationParameters = new TokenValidationParameters
                             {
                                 ValidateIssuer = true,
                                 ValidateAudience = true,
                                 ValidateLifetime = true,
                                 ValidateIssuerSigningKey = true,
                                 ValidIssuer = jwtIssuer,
                                 ValidAudience = jwtIssuer,
                                 IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                             };
                         });

                         builder.Services.AddCors(policyBuilder =>
                            policyBuilder.AddDefaultPolicy(policy =>
                                policy.WithOrigins("*").AllowAnyHeader().AllowAnyMethod())
                        );



                        builder.Services.AddSingleton<StatelessServiceContext>(serviceContext);
                        builder.WebHost
                                    .UseKestrel()
                                    .UseContentRoot(Directory.GetCurrentDirectory())
                                    .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                                    .UseUrls(url);
                        builder.Services.AddControllers();
                        builder.Services.AddEndpointsApiExplorer();
                        builder.Services.AddSwaggerGen();
                        var app = builder.Build();

                        app.UseCors();
                        if (app.Environment.IsDevelopment())
                        {
                        app.UseSwagger();
                        app.UseSwaggerUI();
                        }
                        app.UseAuthentication();
                        app.UseAuthorization();

                        var webSocketOptions = new WebSocketOptions()
                        {
                            KeepAliveInterval = TimeSpan.FromMinutes(2)
                        };

                        app.UseWebSockets(webSocketOptions);

                        app.Use(async (context, next) =>
{
                        if (context.Request.Path == "/ws")
                        {
                            if (context.WebSockets.IsWebSocketRequest)
                            {
                                var token = context.Request.Query["token"].ToString(); 
                                var orderId = context.Request.Query["orderId"].ToString();
                                IOrder proxyOrder = ServiceProxy.Create<IOrder>(new Uri("fabric:/TaxiApp/OrderService"), new ServicePartitionKey(1));
                                var currentOrder = await proxyOrder.GetInfoOfOrderAsync(orderId);

                                if (!string.IsNullOrEmpty(token))
                                {
                                    var handler = new JwtSecurityTokenHandler();
                                    var validationParameters = new TokenValidationParameters
                                    {
                                        ValidateIssuer = true,
                                        ValidateAudience = true,
                                        ValidateLifetime = true,
                                        ValidateIssuerSigningKey = true,
                                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                                        ValidAudience = builder.Configuration["Jwt:Issuer"],
                                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                                    };
                                    try
                                    {
                                        var claimsPrincipal = handler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                                        context.User = claimsPrincipal; 
                                    }
                                    catch (Exception)
                                    {
                                        context.Response.StatusCode = 401; 
                                        return;
                                    }

                                    var email = context.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                                    var role = context.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                                    var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                                    _webSocketManager.AddConnection(email, webSocket);

                                    var secondUser = "";
                                    if (role == "Driver")
                                    {
                                       secondUser = currentOrder.UserId;
                                    }
                                    else
                                    {
                                        secondUser = currentOrder.DriverId;
                                    }
                                    await HandleWebSocketAsync(webSocket, email, secondUser);
                                }
                                else
                                {
                                    context.Response.StatusCode = 401; 
                                }
                            }
                            else
                            {
                                context.Response.StatusCode = 400;
                            }
                        }
                        else
                        {
                            await next();
                        }
                    });
                        app.MapControllers();

                        return app;

                    }))
            };
        }
        private static async Task HandleWebSocketAsync(WebSocket webSocket, string userId, string driverId)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = null;

            IChat chatProxy = ServiceProxy.Create<IChat>(new Uri("fabric:/TaxiApp/ChatService"), new ServicePartitionKey(1));

            var previousMessages = await chatProxy.GetMessageAsync(userId, driverId);
            await SendMessageToClientAsync(webSocket, previousMessages);

            while (true)
            {
                try
                {
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        break; 
                    }
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    var messageDto = JsonConvert.DeserializeObject<ChatMessageDto>(message);
                    await chatProxy.PostMessageAsync(messageDto);
                    var recipientWebSocket = _webSocketManager.GetConnection(messageDto.ReceiverId);

                    if (recipientWebSocket != null && recipientWebSocket.State == WebSocketState.Open)
                    {
                        var responseMessage = Encoding.UTF8.GetBytes(message);
                        await recipientWebSocket.SendAsync(new ArraySegment<byte>(responseMessage, 0, responseMessage.Length),
                            result.MessageType, result.EndOfMessage, CancellationToken.None);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception in WebSocket handling: {ex.Message}");
                    break;
                }
            }
            _webSocketManager.RemoveConnection(userId);
            if (result != null && result.CloseStatus.HasValue)
            {
                await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            }
            else
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Unexpected error", CancellationToken.None);
            }
        }
        private static async Task SendMessageToClientAsync(WebSocket webSocket, IEnumerable<ChatMessageDto> messages)
        {
            var responseMessages = JsonConvert.SerializeObject(messages);
            var responseMessageBytes = Encoding.UTF8.GetBytes(responseMessages);

            await webSocket.SendAsync(new ArraySegment<byte>(responseMessageBytes, 0, responseMessageBytes.Length),
                WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
