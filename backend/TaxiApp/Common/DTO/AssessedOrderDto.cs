﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Common.Models;
using System.Threading.Tasks;

namespace Common.DTO
{
    [DataContract]
    public class AssessedOrderDto
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
        public string? UserId { get; set; }

        public AssessedOrderDto(Order order)
        {
            Id = order.Id;
            StartAddress = order.StartAddress;
            ArriveAddress = order.ArriveAddress;
            Distance = order.Distance;
            Price = order.Price;
            ScheduledPickup = order.ScheduledPickup;
            UserId = order.UserId;
        }
    }
}
