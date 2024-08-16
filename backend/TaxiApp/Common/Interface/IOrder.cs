﻿using Common.DTO;
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
        Task<OrderEstimateDto?> CreateOrderRequestAsync(NewOrderDto data, string email);
        Task<OrderEstimateDto?> GetEstimateOrderAsync(string orderId);
        Task<bool> ConfirmOrderReqAsync(string orderId, string email);
        Task<bool> DeleteOrderReqAsync(string orderId, string email);
        Task<IEnumerable<OrderInfoDto>> GetPreviousOrdersOfUserAsync(string email);


        Task<bool> AcceptOrderAsync(string orderId, string email);
        Task<OrderInfoDto?> GetInfoOfOrderAsync(string orderId);
        Task<bool> FinishOrderAsync(string orderId, string driverId);
    }
}
