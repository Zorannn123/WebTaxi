using Common.DTO;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interface
{
    public interface IOrder : IService
    {
        Task<AssessedOrderDto?> CreateOrderRequestAsync(AssessedOrderDto data, string userId);
    }
}
