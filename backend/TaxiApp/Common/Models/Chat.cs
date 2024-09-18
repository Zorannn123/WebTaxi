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
    public class Chat
    {
        [DataMember]
        public string? Id { get; set; }
        [DataMember]
        public string? SenderId { get; set; }
        [DataMember]
        public string? ReceiverId { get; set; }
        [DataMember]
        public string? Message { get; set; }
        [DataMember]
        public DateTimeOffset SentAt { get; set; }

        public Chat() { }
        public Chat(ChatEntity entity)
        {
            Id = entity.Id;
            SenderId = entity.SenderId;
            ReceiverId = entity.ReceiverId;
            Message = entity.Message;
            SentAt = entity.SentAt;
        }
    }
}
