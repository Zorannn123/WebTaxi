using Common.DTO;
using Common.Interface;
using Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System.Security.Claims;

namespace APIGateway.Controllers
{
    [ApiController]
    [Route("order")]
    public class OrderController : ControllerBase
    {
        [HttpPost]
        [Route("createNew")]
        public async Task<IActionResult> CreateNewOrder(AssessedOrderDto order)
        {
            try
            {
                var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

                IUser userProxy = ServiceProxy.Create<IUser>(new Uri("fabric:/TaxiApp/UserService"), new ServicePartitionKey(1));
                var busy = await userProxy.GetBusyAsync(email);

                if (busy)
                {
                    return Unauthorized();
                }

                IOrder proxy = ServiceProxy.Create<IOrder>(new Uri("fabric:/TaxiApp/OrderService"), new ServicePartitionKey(1));
                var newOrder = proxy.CreateOrderRequestAsync(order, email);

                return Ok(newOrder);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
