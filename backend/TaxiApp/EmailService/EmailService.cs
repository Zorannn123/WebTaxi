using System.Fabric;
using Common.Interface;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using MimeKit;
using MailKit.Security;
using MailKit.Net.Smtp;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Runtime;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;

namespace EmailService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class EmailService : StatelessService, IEmail
    {
        private const string fromAddress = "tadicz84@gmail.com";
        private const string appPassword = "gdtu pmun pfnk zayl";
        public EmailService(StatelessServiceContext context)
            : base(context)
        { }
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return this.CreateServiceRemotingInstanceListeners();
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            long iterations = 0;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ServiceEventSource.Current.ServiceMessage(this.Context, "Working-{0}", ++iterations);

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }

        public async Task SendMailAsync(string emailAddress, bool isApproved)
        {
            var messageBody = "Your account has been";
            if (isApproved)
            {
                messageBody += " approved!:)";
            }
            else
            {
                messageBody += " denied!:(";
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Administrator", fromAddress));
            message.To.Add(new MailboxAddress("", emailAddress));
            message.Subject = "Driver verification status";
            message.Body = new TextPart("plain") { Text = messageBody };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(fromAddress, appPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
    }
}
