using Common.DTO;
using Common.TableStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    [DataContract]
    public class Rating
    {
        [DataMember]
        public string? Id { get; set; } 
        [DataMember]
        public int Rate { get; set; }
        [DataMember]
        public string? UserId { get; set; }
        [DataMember]
        public string? DriverId { get; set; }

        public Rating(RateDto data, string userId)
        {
            Id = data.OrderId;
            Rate = data.Rate;
            DriverId = data.DriverId;
            UserId = userId;
            }

        public Rating(RatingEntity entity)
        {
            Id = entity.Id;
            Rate = entity.Rate;
            UserId = entity.UserId;
            DriverId = entity.DriverId;
        }
        
    }
}
