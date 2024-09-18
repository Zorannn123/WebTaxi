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
    public class ChatEntity : ITableEntity
    {
        public string? Id { get; set; }
        public string? SenderId { get; set; }
        public string? ReceiverId { get; set; }
        public string? Message { get; set; }
        public DateTimeOffset SentAt { get; set; }
        public string PartitionKey { get; set; } = "Chat";
        public string RowKey { get; set; } = null!;
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public ChatEntity()
        {
            PartitionKey = "Chat";
            Timestamp = DateTimeOffset.Now;
            ETag = ETag.All;
        }

        public ChatEntity(Chat chat)
        {
            RowKey = chat.Id;
            Id = chat.Id;
            SenderId = chat.SenderId;
            ReceiverId = chat.ReceiverId;
            Message = chat.Message;
            SentAt = chat.SentAt;
        }
    }
}
