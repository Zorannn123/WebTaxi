using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Models;
using Azure.Data.Tables;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Common.Interface;
using Common.TableStorage;
using System.Security.Principal;
using Common.DTO;
using Common.Enum;
using Microsoft.ServiceFabric.Services.Client;

namespace OrderService
{
    internal sealed class OrderService : StatefulService, IOrder
    {
        private TableClient orderTable = null!;
        private Thread orderTableThread = null!;
        private IReliableDictionary<string, Order> orderDictionary = null!; 

        public OrderService(StatefulServiceContext context)
            : base(context)
        { }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            await SetRideTableAsync();
            orderDictionary = await StateManager.GetOrAddAsync<IReliableDictionary<string, Order>>("OrderDictionary");
            var orders = await GetOrdersFromTableAsync();

            using (var transaction = StateManager.CreateTransaction())
            {

                foreach (var order in orders)
                {
                    await orderDictionary.TryAddAsync(transaction, order.Id, order);
                }
                await transaction.CommitAsync();
            }

            orderTableThread = new Thread(new ThreadStart(RideTableWriteThread));
            orderTableThread.Start();
        }

        public async Task<bool> UpdateStatuses()
        {
            bool result = false;

            using (var transaction = StateManager.CreateTransaction())
            {
                var ordersToUpdate = new List<string>();

                // Retrieve all orders using an enumerator
                var enumerator = (await orderDictionary.CreateEnumerableAsync(transaction)).GetAsyncEnumerator();
                while (await enumerator.MoveNextAsync(CancellationToken.None))
                {
                    var orderId = enumerator.Current.Key;
                    var order = enumerator.Current.Value;
                    var now = DateTime.UtcNow;

                    // Check if order is WaitingForPickup and update to InProgress if 5 seconds have passed
                    if (order.Status == OrderStatus.WaitingForPickup &&
                        (now - order.StartingTime).TotalSeconds >= order.ScheduledPickup)
                    {
                        order.Status = OrderStatus.InProgress;
                        ordersToUpdate.Add(orderId);
                    }

                    // Check if order is InProgress and update to Finished if 15 seconds have passed
                    if (order.Status == OrderStatus.InProgress &&
                        (now - order.StartingTime).TotalSeconds >= order.Distance + order.ScheduledPickup)
                    {
                        order.Status = OrderStatus.Finished;
                        ordersToUpdate.Add(orderId);


                        IUser proxy = ServiceProxy.Create<IUser>(new Uri("fabric:/TaxiApp/UserService"), new ServicePartitionKey(1));
                        await proxy.ChangeBusyAsync(order.DriverId, false);
                        await proxy.ChangeBusyAsync(order.UserId, false);
                    }
                }

                // Apply updates in a single transaction
                foreach (var orderId in ordersToUpdate)
                {
                    var order = await orderDictionary.TryGetValueAsync(transaction, orderId);
                    if (order.HasValue)
                    {
                        await orderDictionary.TryUpdateAsync(transaction, orderId, order.Value, order.Value);
                    }
                }

                try
                {
                    await transaction.CommitAsync();
                    result = true;
                }
                catch (Exception)
                {
                    transaction.Abort();
                    result = false;
                }
            }

            return result;
        }


        private async Task SetRideTableAsync()
        {
            var tableServiceClient = new TableServiceClient("UseDevelopmentStorage=true");
            await tableServiceClient.CreateTableIfNotExistsAsync("Order");
            orderTable = tableServiceClient.GetTableClient("Order");
        }
        private async Task<IEnumerable<Order>> GetOrdersFromTableAsync()
        {
            var orders = new List<Order>();

            await foreach (var entity in orderTable.QueryAsync<OrderEntity>(filter: x => true))
            {
                var order = new Order(entity);
                orders.Add(order);
            }

            return orders;
        }


        private async void RideTableWriteThread()
        {
            while (true)
            {
                using (var transaction = StateManager.CreateTransaction())
                {
                    var enumerator = (await orderDictionary.CreateEnumerableAsync(transaction)).GetAsyncEnumerator();

                    while (await enumerator.MoveNextAsync(CancellationToken.None))
                    {
                        var order = enumerator.Current.Value;
                        var orderEntity = new OrderEntity(order);
                        await orderTable.UpsertEntityAsync(orderEntity, TableUpdateMode.Merge, CancellationToken.None);
                    }
                }

                Thread.Sleep(5000);
            }
        }

        public async Task<OrderInfoDto?> GetInfoOfOrderAsync(string orderId)
        {
            using (var transaction = StateManager.CreateTransaction())
            {
                var currOrder = await orderDictionary.TryGetValueAsync(transaction, orderId);

                if (currOrder.HasValue)
                {
                    Order order = currOrder.Value;
                    OrderInfoDto infoOfOrder = new OrderInfoDto(order);

                    return infoOfOrder;
                }

                return null;
            }
        }

        #region USER
        public async Task<OrderEstimateDto?> CreateOrderRequestAsync(NewOrderDto data, string userId)
        {
            using (var transaction = StateManager.CreateTransaction())
            {
                Order order = new Order(data.StartAddress, data.ArriveAddress, userId);

                try
                {
                    await orderDictionary.AddAsync(transaction, order.Id, order);
                    await transaction.CommitAsync();
                    return new OrderEstimateDto(order);
                }
                catch (Exception)
                {
                    transaction.Abort();
                }

            }

            return null;
        }

        public async Task<OrderEstimateDto?> GetEstimateOrderAsync(string id)
        {
            using (var transaction = StateManager.CreateTransaction())
            {
                var currOrder = await orderDictionary.TryGetValueAsync(transaction, id);
                if (currOrder.HasValue)
                {
                    OrderEstimateDto orderDto = new OrderEstimateDto(currOrder.Value);

                    return orderDto;
                }
                return null;
            }
        }

        public async Task<bool> ConfirmOrderReqAsync(string orderId, string userId)
        {
            bool temp = false;
            using (var transaction = StateManager.CreateTransaction())
            {
                var orderRes = await orderDictionary.TryGetValueAsync(transaction, orderId);
                if (orderRes.HasValue)
                {
                    var order = orderRes.Value;
                    if (orderRes.Value.Status == OrderStatus.OnHold && order.UserId.Equals(userId))
                    {
                        orderRes.Value.Status = OrderStatus.ConfirmedByUser;
                        try
                        {
                            await orderDictionary.TryUpdateAsync(transaction, orderId, order, order);
                            await transaction.CommitAsync();
                            temp = true;
                        }
                        catch (Exception ex)
                        {
                            temp = false;
                            transaction.Abort();
                        }
                    }
                }
            }
            return temp;
        }

        public async Task<bool> DeleteOrderReqAsync(string orderId, string userId)
        {
            bool temp = false;

            using (var transaction = StateManager.CreateTransaction())
            {
                var orderRes = await orderDictionary.TryGetValueAsync(transaction, orderId);

                if (orderRes.HasValue)
                {
                    try
                    {
                        await orderDictionary.TryRemoveAsync(transaction, orderId);
                        await orderTable.DeleteEntityAsync("Order", orderId);
                        await transaction.CommitAsync();
                        temp = true;
                    }
                    catch (Exception)
                    {
                        temp = false;
                        transaction.Abort();
                    }
                }
            }
            return temp;
        }

        public async Task<IEnumerable<OrderInfoDto>> GetPreviousOrdersOfUserAsync(string email)
        {
            List<OrderInfoDto> orders = new List<OrderInfoDto>();

            using (var transaction = StateManager.CreateTransaction())
            {
                var enums = (await orderDictionary.CreateEnumerableAsync(transaction)).GetAsyncEnumerator();
                while(await enums.MoveNextAsync(CancellationToken.None))
                {
                    var temp = enums.Current.Value;
                    if(temp.Status == OrderStatus.Finished && temp.UserId != null && temp.UserId.Equals(email))
                    {
                        orders.Add(new OrderInfoDto(temp));
                    }
                }
            }
            return orders;
        }
        #endregion
        #region DRIVER

        public async Task<bool> AcceptOrderAsync(string orderId, string email)
        {
            bool temp = false;
            using (var transaction = StateManager.CreateTransaction())
            {
                var currOrder = await orderDictionary.TryGetValueAsync(transaction, orderId);

                if (currOrder.HasValue)
                {
                    var order = currOrder.Value;

                    if (currOrder.Value.Status == OrderStatus.ConfirmedByUser)
                    {
                        currOrder.Value.Status = OrderStatus.WaitingForPickup;
                        currOrder.Value.DriverId = email;
                        currOrder.Value.StartingTime = DateTime.UtcNow;

                        try
                        {
                            await orderDictionary.TryUpdateAsync(transaction, orderId, order, order);
                            await transaction.CommitAsync();
                            temp = true;
                        }
                        catch (Exception)
                        {
                            temp = false;
                            transaction.Abort();
                        }
                    }
                }
            }
            return temp;
        }

        public async Task<bool> FinishOrderAsync(string orderId, string driverId)
        {
            bool temp = false;

            using (var transaction = StateManager.CreateTransaction())
            {
                var currOrder = await orderDictionary.TryGetValueAsync(transaction, orderId);

                if (currOrder.HasValue)
                {
                    var order = currOrder.Value;

                    if (currOrder.Value.Status == OrderStatus.InProgress && order.DriverId != null && order.DriverId.Equals(driverId))
                    {
                        var completedRide = order;
                        completedRide.Status = OrderStatus.Finished;

                        try
                        {
                            await orderDictionary.TryUpdateAsync(transaction, orderId, completedRide, order);
                            await transaction.CommitAsync();
                            temp = true;
                        }
                        catch (Exception)
                        {
                            temp = false;
                            transaction.Abort();
                        }
                    }
                }
            }

            return temp;
        }


        public async Task<IEnumerable<OrderInfoDto>> GetPreviousOrderForDriverAsync(string driverId)
        {
            List<OrderInfoDto> orders = new List<OrderInfoDto>();

            using (var transaction = StateManager.CreateTransaction())
            {
                var enums = (await orderDictionary.CreateEnumerableAsync(transaction)).GetAsyncEnumerator();

                while (await enums.MoveNextAsync(CancellationToken.None))
                {
                    var currOrder = enums.Current.Value;
                    if (currOrder.Status == OrderStatus.Finished && currOrder.DriverId != null && currOrder.DriverId.Equals(driverId))
                    {
                        orders.Add(new OrderInfoDto(currOrder));
                    }
                }
            }

            return orders;
        }

        public async Task<IEnumerable<OrderInfoDto>> GetAllOnHoldOrdersAsync()
        {
            List<OrderInfoDto> orders = new List<OrderInfoDto>();

            using (var transaction = StateManager.CreateTransaction())
            {
                var enums = (await orderDictionary.CreateEnumerableAsync(transaction)).GetAsyncEnumerator();

                while (await enums.MoveNextAsync(CancellationToken.None))
                {
                    var currOrder = enums.Current.Value;
                    if (currOrder.Status == OrderStatus.ConfirmedByUser)
                    {
                        orders.Add(new OrderInfoDto(currOrder));
                    }
                }
            }
            return orders;
        }

        #endregion
        #region ADMIN
        public async Task<IEnumerable<OrderInfoDto>> GetAllOrdersAsync()
        {
            List<OrderInfoDto> orders = new List<OrderInfoDto>();

            using (var tx = StateManager.CreateTransaction())
            {
                var enums = (await orderDictionary.CreateEnumerableAsync(tx)).GetAsyncEnumerator();

                while (await enums.MoveNextAsync(CancellationToken.None))
                {
                    var temp = enums.Current.Value;
                    var order = new OrderInfoDto(temp);
                    orders.Add(order);
                }
            }

            return orders;
        }
        #endregion
    }
}
