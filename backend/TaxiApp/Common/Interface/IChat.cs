using Common.DTO;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interface
{
    public interface IChat : IService
    {
        Task PostMessageAsync(ChatMessageDto messageDto);
        Task<IEnumerable<ChatMessageDto>> GetMessageAsync(string senderId, string receiverId);
    }
}
