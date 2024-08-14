using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.DTO;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Common.Interface
{
    public interface IUser : IService
    {
        Task<bool> LoginAsync(LoginDto logDto);
        Task<bool> RegisterAsync(RegisterDto registerDto);
        Task<UserDto> GetCurrentUserAsync(string email);
        Task<bool> EditProfileAsync(UserDto userDto);
        Task<string> GetUserType(string email);


        Task<bool> ApproveVerificationAsync(string id);
        Task<bool> DenyVerificationAsync(string id);
        Task<IEnumerable<DriverDto>> GetDriversAsync();
        Task<bool> DriverBlockAsync(string id);
    }
}
