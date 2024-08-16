using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO
{
    [DataContract]
    public class NewOrderDto
    {
        [DataMember]
        public string? StartAddress { get; set; }
        [DataMember]
        public string? ArriveAddress { get; set; }
    }
}
