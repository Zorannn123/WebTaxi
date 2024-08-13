using Common.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.AspNetCore.Authorization;

namespace APIGateway.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("administrator")]
    public class AdministratorController : ControllerBase
    {
        [HttpPost]
        [Route("approveVerification")]
        public async Task<IActionResult> ApproveVerificationAsync(string id)
        {
            try
            {
                IUser proxy = ServiceProxy.Create<IUser>(new Uri("fabric:/TaxiApp/UserService"), new ServicePartitionKey(1));
                var retVal = await proxy.ApproveVerificationAsync(id);
                return Ok(retVal);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("denyVerification")]
        public async Task<IActionResult> DenyVerificationAsync(string id)
        {
            try
            {
                IUser proxy = ServiceProxy.Create<IUser>(new Uri("fabric:/TaxiApp/UserService"), new ServicePartitionKey(1));
                var retVal = await proxy.DenyVerificationAsync(id);
                return Ok(retVal);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
