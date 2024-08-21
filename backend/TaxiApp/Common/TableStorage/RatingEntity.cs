using Azure;
using Azure.Data.Tables;
using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.TableStorage
{
    public class RatingEntity : ITableEntity
    {
        public string? Id { get; set; }
        public int Rate { get; set; }
        public string? UserId { get; set; }
        public string? DriverId { get; set; }

        public string PartitionKey { get; set; } = "Rating";
        public string RowKey { get; set; } = null!;
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public RatingEntity()
        {
            PartitionKey = "Rating";
            Timestamp = DateTimeOffset.Now;
            ETag = ETag.All;
        }

        public RatingEntity(Rating rating)
        {
            RowKey = rating.Id;
            Id = rating.Id;
            Rate = rating.Rate;
            UserId = rating.UserId;
            DriverId = rating.DriverId;
        }
    }
}
