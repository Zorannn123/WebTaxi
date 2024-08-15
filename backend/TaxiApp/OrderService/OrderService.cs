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

        public async Task<AssessedOrderDto?> CreateOrderRequestAsync(AssessedOrderDto data, string userId)
        {
            using (var transaction = StateManager.CreateTransaction())
            {
                Order order = new Order(data.StartAddress, data.ArriveAddress, userId);

                try
                {
                    await orderDictionary.AddAsync(transaction, order.Id, order);
                    await transaction.CommitAsync();
                    return new AssessedOrderDto(order);
                }
                catch (Exception)
                {
                    transaction.Abort();
                }

            }

            return null;
        }
    }
}
