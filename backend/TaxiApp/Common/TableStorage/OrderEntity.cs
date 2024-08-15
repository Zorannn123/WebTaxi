using Azure;
using Azure.Data.Tables;
using Common.Enum;
using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.TableStorage
{
    public class OrderEntity : ITableEntity
    {
        [DataMember]
        public string? Id { get; set; }
        [DataMember]
        public string? StartAddress { get; set; }
        [DataMember]
        public string? ArriveAddress { get; set; }
        [DataMember]
        public double Distance { get; set; }   
        [DataMember]
        public float Price { get; set; }
        [DataMember]
        public int RideDuration { get; set; }
        [DataMember]
        public int ScheduledPickup { get; set; }     
        [DataMember]
        public DateTime StartingTime { get; set; }
        [DataMember]
        public OrderStatus Status { get; set; }    
        [DataMember]
        public string? UserId { get; set; }
        [DataMember]
        public string? DriverId { get; set; } = null;

        public string PartitionKey { get; set; } = "Order";
        public string RowKey { get; set; } = null!;
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public OrderEntity()
        {
            PartitionKey = "Ride";
            Timestamp = DateTimeOffset.Now;
            ETag = ETag.All;
        }

        public OrderEntity(Order order)
        {
            RowKey = order.Id;
            Id = order.Id;
            StartAddress = order.StartAddress;
            ArriveAddress = order.ArriveAddress;
            Distance = order.Distance;
            Price = order.Price;
            ScheduledPickup = order.ScheduledPickup;
            RideDuration = order.RideDuration;
            StartingTime = order.StartingTime;
            Status = order.Status;
            UserId = order.UserId;
            DriverId = order.DriverId;
        }
    }
}
