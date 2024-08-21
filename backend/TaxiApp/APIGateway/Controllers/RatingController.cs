using Common.DTO;
using Common.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System.Security.Claims;

namespace APIGateway.Controllers
{
    [ApiController]
    [Route("rate")]
    public class RatingController : ControllerBase
    {
        [Authorize(Roles = "User")]
        [HttpPost]
        [Route("rateRide")]
        public async Task<IActionResult> RateRideAsync(RateDto rate)
        {
            try
            {
                var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

                IRating proxy = ServiceProxy.Create<IRating>(new Uri("fabric:/TaxiApp/RatingService"), new ServicePartitionKey(1));
                var retData = await proxy.RateRideAsync(rate, email);

                return Ok(retData);
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                return BadRequest(message);
            }
        }
        //[Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("getRating")]
        public async Task<IActionResult> GetRatingDriverAsync(string driverId)
        {
            try
            {
                //var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

                IRating proxy = ServiceProxy.Create<IRating>(new Uri("fabric:/TaxiApp/RatingService"), new ServicePartitionKey(1));
                var temp = await proxy.GetAverageRateDriverAsync(driverId);

                return Ok(temp);
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                return BadRequest(message);
            }
        }
    }
}
