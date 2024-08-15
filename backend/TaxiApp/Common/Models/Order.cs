using Common.Enum;
using Common.TableStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    [DataContract]
    public class Order
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
        public int ScheduledPickup { get; set; }     
        [DataMember]
        public int RideDuration { get; set; }      
        [DataMember]
        public DateTime StartingTime { get; set; }
        [DataMember]
        public OrderStatus Status { get; set; }    
        [DataMember]
        public string? UserId { get; set; }
        [DataMember]
        public string? DriverId { get; set; } = null;

        public Order(OrderEntity order)
        {
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

        public Order(string? startAddress, string? arriveAddress, string? userId)
        {
            Random rand = new Random();

            Id = Guid.NewGuid().ToString();
            StartAddress = startAddress;
            ArriveAddress = ArriveAddress;
            Distance = rand.Next(1, 5);
            Price = rand.Next(190, 700);
            ScheduledPickup = rand.Next(3, 11);
            RideDuration = rand.Next(2, 10);
            StartingTime = DateTime.UtcNow.AddMinutes(ScheduledPickup); 
            Status = OrderStatus.OnHold;
            UserId = UserId;
            DriverId = null;
        }
    }
}
