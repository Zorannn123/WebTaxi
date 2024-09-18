using Microsoft.AspNetCore.Mvc;
using Common.DTO;
using Common.Interface;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Common.Models;

namespace APIGateway.Controllers
{
    [Authorize]
    [ApiController]
    [Route("user")]
    public class UserController : ControllerBase
    {
        [HttpPost]
        [Route("editProfile")]
        public async Task<IActionResult> EditProfileAsync([FromBody]UserDto userData)
        {
            try
            {
                IUser proxy = ServiceProxy.Create<IUser>(new Uri("fabric:/TaxiApp/UserService"), new ServicePartitionKey(1));
                var temp = await proxy.EditProfileAsync(userData);
                return Ok(temp);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
         
        [HttpGet]
        [Route("currentProfile")]
        public async Task<IActionResult> GetProfileAsync()
        {
            try
            {
                var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

                if (string.IsNullOrEmpty(email))
                {
                    return Unauthorized("Invalid token, email claim not found.");
                }

                IUser proxy = ServiceProxy.Create<IUser>(new Uri("fabric:/TaxiApp/UserService"), new ServicePartitionKey(1));
                var userProfile = await proxy.GetCurrentUserAsync(email);

                if (userProfile != null)
                {
                    return Ok(userProfile);
                }
                else
                {
                    return NotFound("User profile not found.");
                }
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                return BadRequest(message);
            }
        }

        [HttpGet]
        [Route("userType")]
        public async Task<IActionResult> GetUserTypeAsync()
        {
            try
            {
                var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                IUser proxy = ServiceProxy.Create<IUser>(new Uri("fabric:/TaxiApp/UserService"), new ServicePartitionKey(1));
                var temp = await proxy.GetUserType(email);

                return Ok(temp);
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                return BadRequest(message);
            }
        }

        [HttpGet]
        [Route("isVerified")]
        public async Task<IActionResult> IsVerifiedAsync()
        {
            try
            {
                var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                IUser proxy = ServiceProxy.Create<IUser>(new Uri("fabric:/TaxiApp/UserService"), new ServicePartitionKey(1));
                var retVal = await proxy.IsVerifiedAsync(email);
                return Ok(retVal);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("isBlocked")]
        public async Task<IActionResult> IsBlockedAsync()
        {
            try
            {
                var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                IUser proxy = ServiceProxy.Create<IUser>(new Uri("fabric:/TaxiApp/UserService"), new ServicePartitionKey(1));
                var retVal = await proxy.IsBlockedAsync(email);
                return Ok(retVal);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet]
        [Route("busyStatus")]
        public async Task<IActionResult> GetBusy()
        {
            try
            {
                var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                IUser proxy = ServiceProxy.Create<IUser>(new Uri("fabric:/TaxiApp/UserService"), new ServicePartitionKey(1));
                var temp = await proxy.GetBusyAsync(userEmail);

                return Ok(temp);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        /*[AllowAnonymous]
        [HttpPost]
        [Route("sendMessage")]
        public async Task<IActionResult> StoreMessageAsync([FromBody]ChatMessageDto messageDto)
        {
            try
            {
                IChat proxy = ServiceProxy.Create<IChat>(new Uri("fabric:/TaxiApp/ChatService"), new ServicePartitionKey(1));
                await proxy.PostMessageAsync(messageDto);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }*/
    }
}
