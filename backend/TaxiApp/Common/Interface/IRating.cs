using Common.DTO;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interface
{
    public interface IRating : IService
    {
        Task<bool> RateRideAsync(RateDto rate, string userId);
        Task<float> GetAverageRateDriverAsync(string driverId);
    }
}
