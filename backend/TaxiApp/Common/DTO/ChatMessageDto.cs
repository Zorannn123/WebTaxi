using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO
{
    [DataContract]
    public class ChatMessageDto
    {
        [DataMember]
        public string? SenderId { get; set; }
        [DataMember]
        public string? ReceiverId { get; set; }
        [DataMember]
        public string? Message { get; set; }
        [DataMember]
        public DateTimeOffset SentAt { get; set; }
    }
}
