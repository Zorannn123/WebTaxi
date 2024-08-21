using Common.DTO;
using Common.Interface;
using Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System.Fabric;
using System.Security.Claims;

namespace APIGateway.Controllers
{
    [ApiController]
    [Route("order")]
    public class OrderController : ControllerBase
    {
        [Authorize(Roles = "User")]
        [HttpPost]
        [Route("createNew")]
        public async Task<IActionResult> CreateNewOrder(NewOrderDto order)
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
                var newOrder = await proxy.CreateOrderRequestAsync(order, email);

                return Ok(newOrder);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "User")]
        [HttpGet]
        [Route("estimateOrder")]
        public async Task<IActionResult> GetEstimateOrder(string orderId) 
        {
            try
            {
                IOrder proxy = ServiceProxy.Create<IOrder>(new Uri("fabric:/TaxiApp/OrderService"), new ServicePartitionKey(1));
                var newOrder = await proxy.GetEstimateOrderAsync(orderId);

                return Ok(newOrder);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "User")]
        [HttpPost]
        [Route("confirmOrder")]
        public async Task<IActionResult> ConfirmOrder(string orderId)
        {
            try
            {
                var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;


                IUser userProxy = ServiceProxy.Create<IUser>(new Uri("fabric:/TaxiApp/UserService"), new ServicePartitionKey(1));
                var busy = await userProxy.GetBusyAsync(email);

                if (busy)
                {
                    return Unauthorized("User is currently busy and cannot confirm the order.");
                }

                IOrder proxy = ServiceProxy.Create<IOrder>(new Uri("fabric:/TaxiApp/OrderService"), new ServicePartitionKey(1));
                var confirmationResult = await proxy.ConfirmOrderReqAsync(orderId, email);

                if (confirmationResult)
                {
                    return Ok("Order confirmed successfully.");
                }
                else
                {
                    return BadRequest("Failed to confirm the order.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "User")]
        [HttpPost]
        [Route("deleteOrder")]
        public async Task<IActionResult> DeleteOrder(string orderId)
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
                var retData = await proxy.DeleteOrderReqAsync(orderId, email);

                return Ok(retData);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "User")]
        [HttpGet]
        [Route("previousOrders")]
        public async Task<IActionResult> GetPrevious()
        {
            try
            {
                var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                IOrder proxy = ServiceProxy.Create<IOrder>(new Uri("fabric:/TaxiApp/OrderService"), new ServicePartitionKey(1));
                var retVal = await proxy.GetPreviousOrdersOfUserAsync(email);
                return Ok(retVal);
            }catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "Driver")]
        [HttpPost]
        [Route("acceptOrder")]
        public async Task<IActionResult> AcceptOrder(string orderId)
        {
            try
            {
                var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

                IUser userProxy = ServiceProxy.Create<IUser>(new Uri("fabric:/TaxiApp/UserService"), new ServicePartitionKey(1));
                var isVerified = await userProxy.IsVerifiedAsync(email);
                var isBlocked = await userProxy.IsBlockedAsync(email);
                var isBusy = await userProxy.GetBusyAsync(email);

                if (!isVerified || isBlocked || isBusy)
                {
                    return Unauthorized();
                }

                IOrder proxy = ServiceProxy.Create<IOrder>(new Uri("fabric:/TaxiApp/OrderService"), new ServicePartitionKey(1));
                var acceptOrder = await proxy.AcceptOrderAsync(orderId, email);
                var infoOfOrder = await proxy.GetInfoOfOrderAsync(orderId);

                if (acceptOrder)
                {
                    await userProxy.ChangeBusyAsync(infoOfOrder.DriverId, true);
                    await userProxy.ChangeBusyAsync(infoOfOrder.UserId, true);
                }
                return Ok(acceptOrder);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "Driver")]
        [HttpGet]
        [Route("onHoldOrders")]
        public async Task<IActionResult> GetAllOnHold()
        {
            try
            {
                var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

                IUser userProxy = ServiceProxy.Create<IUser>(new Uri("fabric:/TaxiApp/UserService"), new ServicePartitionKey(1));
                var isBusy = await userProxy.GetBusyAsync(email);

                if (isBusy)
                {
                    return StatusCode(StatusCodes.Status423Locked, "You are currently busy. Please try again later.");
                }

                IOrder proxy = ServiceProxy.Create<IOrder>(new Uri("fabric:/TaxiApp/OrderService"), new ServicePartitionKey(1));
                var orders = await proxy.GetAllOnHoldOrdersAsync();

                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "Driver")]
        [HttpGet]
        [Route("allPreviousOrders")]
        public async Task<IActionResult> GetAllPreviousOrders()
        {
            try
            {
                var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

                IUser userProxy = ServiceProxy.Create<IUser>(new Uri("fabric:/TaxiApp/UserService"), new ServicePartitionKey(1));
                var isVerified = await userProxy.IsVerifiedAsync(email);

                if (!isVerified)
                {
                    return Unauthorized();
                }
                IOrder proxy = ServiceProxy.Create<IOrder>(new Uri("fabric:/TaxiApp/OrderService"), new ServicePartitionKey(1));
                var orders = await proxy.GetPreviousOrderForDriverAsync(email);

                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "Driver")]
        [HttpPost]
        [Route("finishOrder")]
        public async Task<IActionResult> FinishOrder(string orderId)
        {
            try
            {
                var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

                IUser userProxy = ServiceProxy.Create<IUser>(new Uri("fabric:/TaxiApp/UserService"), new ServicePartitionKey(1));
                var isVerified = await userProxy.IsVerifiedAsync(email);
                var isBlocked = await userProxy.IsBlockedAsync(email);

                if (!isVerified || isBlocked)
                {
                    return Unauthorized();
                }

                IOrder proxy = ServiceProxy.Create<IOrder>(new Uri("fabric:/TaxiApp/OrderService"), new ServicePartitionKey(1));
                var completeOrder = await proxy.FinishOrderAsync(orderId, email);
                var infoOfOrder = await proxy.GetInfoOfOrderAsync(orderId);

                if (completeOrder)
                {
                    await userProxy.ChangeBusyAsync(infoOfOrder.DriverId, false);
                    await userProxy.ChangeBusyAsync(infoOfOrder.UserId, false);
                }

                return Ok(completeOrder);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("allOrders")]
        public async Task<IActionResult> GetAllOrders()
        {
            try
            {
                IOrder proxy = ServiceProxy.Create<IOrder>(new Uri("fabric:/TaxiApp/OrderService"), new ServicePartitionKey(1));
                var orders = await proxy.GetAllOrdersAsync();

                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
