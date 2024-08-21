using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO
{
    [DataContract]
    public class RateDto
    {
        [DataMember]
        public string? OrderId { get; set; }
        [DataMember]
        public int Rate { get; set; }
        [DataMember]
        public string? DriverId { get; set; }
    }
}
