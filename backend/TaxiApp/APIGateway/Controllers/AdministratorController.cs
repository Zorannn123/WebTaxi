using Common.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.AspNetCore.Authorization;

namespace APIGateway.Controllers
{
    //[Authorize(Roles = "Admin")]
    [ApiController]
    [Route("administrator")]
    public class AdministratorController : ControllerBase
    {
        [HttpPost]
        [Route("approveVerification")]
        public async Task<IActionResult> ApproveVerificationAsync(string email)
        {
            try
            {
                IUser proxy = ServiceProxy.Create<IUser>(new Uri("fabric:/TaxiApp/UserService"), new ServicePartitionKey(1));
                var retVal = await proxy.ApproveVerificationAsync(email);
                return Ok(retVal);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("denyVerification")]
        public async Task<IActionResult> DenyVerificationAsync(string email)
        {
            try
            {
                IUser proxy = ServiceProxy.Create<IUser>(new Uri("fabric:/TaxiApp/UserService"), new ServicePartitionKey(1));
                var retVal = await proxy.DenyVerificationAsync(email);
                return Ok(retVal);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("drivers")]
        public async Task<IActionResult> GetDriversAsync()
        {
            try
            {
                IUser proxy = ServiceProxy.Create<IUser>(new Uri("fabric:/TaxiApp/UserService"), new ServicePartitionKey(1));
                var retVal = await proxy.GetDriversAsync();
                if(retVal == null || !retVal.Any())
                {
                    return NotFound("No drivers found");
                }

                return Ok(retVal);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("blockDriver")]
        public async Task<IActionResult> DriverBlockAsync(string email)
        {
            try
            {
                IUser proxy = ServiceProxy.Create<IUser>(new Uri("fabric:/TaxiApp/UserService"), new ServicePartitionKey(1));
                var retVal = await proxy.DriverBlockAsync(email);
                return Ok(retVal);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        
    }
}
