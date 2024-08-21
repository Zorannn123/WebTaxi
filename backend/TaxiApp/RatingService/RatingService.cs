using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Common.DTO;
using Common.Interface;
using Common.Models;
using Common.TableStorage;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace RatingService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class RatingService : StatefulService, IRating
    {
        private TableClient ratingTable = null!;
        private Thread ratingTableThread = null!;
        private IReliableDictionary<string, Rating> ratingDictionary = null!;
        public RatingService(StatefulServiceContext context)
            : base(context)
        { }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            await SetRatingTableAsync();
            ratingDictionary = await StateManager.GetOrAddAsync<IReliableDictionary<string, Rating>>("RatingDictionary");

            var ratings = await GetRatingsFromTableAsync();

            using (var transaction = StateManager.CreateTransaction())
            {
                foreach(var rate in ratings)
                {
                    await ratingDictionary.TryAddAsync(transaction, rate.Id, rate);
                }
                await transaction.CommitAsync();
            }

            ratingTableThread = new Thread(new ThreadStart(RatingTableWriteThread));
            ratingTableThread.Start();
        }


        private async Task SetRatingTableAsync()
        {
            var tableServiceClient = new TableServiceClient("UseDevelopmentStorage=true");
            await tableServiceClient.CreateTableIfNotExistsAsync("Rating");
            ratingTable = tableServiceClient.GetTableClient("Rating");
        }

        private async Task<IEnumerable<Rating>> GetRatingsFromTableAsync()
        {
            var ratings = new List<Rating>();

            await foreach (var entity in ratingTable.QueryAsync<RatingEntity>(filter: x => true))
            {
                var order = new Rating(entity);
                ratings.Add(order);
            }

            return ratings;
        }

        private async void RatingTableWriteThread()
        {
            while (true)
            {
                using (var tx = StateManager.CreateTransaction())
                {
                    var enumerator = (await ratingDictionary.CreateEnumerableAsync(tx)).GetAsyncEnumerator();

                    while (await enumerator.MoveNextAsync(CancellationToken.None))
                    {
                        var rating = enumerator.Current.Value;
                        var ratingEntity = new RatingEntity(rating);
                        await ratingTable.UpsertEntityAsync(ratingEntity, TableUpdateMode.Merge, CancellationToken.None);
                    }
                }

                Thread.Sleep(5000);
            }
        }

        public async Task<bool> RateRideAsync(RateDto rate, string userId)
        {
            bool result = false;

            using (var transaction = StateManager.CreateTransaction())
            {
                // Provera da li je neko već ocenio tu vožnju
                var ratingRes = await ratingDictionary.TryGetValueAsync(transaction, rate.OrderId);

                if (!ratingRes.HasValue)
                {
                    Rating newRating = new Rating(rate, userId);

                    try
                    {
                        await ratingDictionary.AddAsync(transaction, rate.OrderId, newRating);
                        await transaction.CommitAsync();
                        result = true;
                    }
                    catch (Exception)
                    {
                        result = false;
                        transaction.Abort();
                    }
                }
            }

            return result;
        }

        public async Task<float> GetAverageRateDriverAsync(string driverId)
        {
            float sum = 0;
            float count = 0;
            float average = 0;  

            using (var transaction = StateManager.CreateTransaction())
            {
                var enumerator = (await ratingDictionary.CreateEnumerableAsync(transaction)).GetAsyncEnumerator();

                var driverRatings = new List<float>();

                while (await enumerator.MoveNextAsync(CancellationToken.None))
                {
                    var temp = enumerator.Current.Value;
                    if (temp.DriverId.Equals(driverId))
                    {
                        sum += temp.Rate;
                        count++;
                    }
                }
            }

            if(count > 0)
            {
                average = sum / count;
            }
            return average;
        }

    }
}
