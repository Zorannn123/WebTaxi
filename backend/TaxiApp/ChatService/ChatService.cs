using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Common.Interface;
using Common.Models;
using Azure.Data.Tables;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Common.TableStorage;
using Common.DTO;
using Common.WebSocketManager;
using System.Net.WebSockets;

namespace ChatService
{

    internal sealed class ChatService : StatefulService, IChat
    {
        private TableClient chatsTable = null;
        private Thread chatsTableThread = null;
        private IReliableDictionary<string, Chat> chatsDictionary = null;
        public ChatService(StatefulServiceContext context)
            : base(context)
        { }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            await SetTableAsync();

            chatsDictionary = await StateManager.GetOrAddAsync<IReliableDictionary<string, Chat>>("ChatsDictionary");
            var chats = await GetChatsFromTableAsync();
            using (var transaction = StateManager.CreateTransaction())
            {

                foreach (var chat in chats)
                {
                    await chatsDictionary.TryAddAsync(transaction, chat.Id, chat);
                }
                await transaction.CommitAsync();
            }


            chatsTableThread = new Thread(new ThreadStart(ChatsTableThread));
            chatsTableThread.Start();
        }

        private async Task SetTableAsync()
        {
            var tsClient = new TableServiceClient("UseDevelopmentStorage=true");
            await tsClient.CreateTableIfNotExistsAsync("Chat");
            chatsTable = tsClient.GetTableClient("Chat");
        }

        private async Task<IEnumerable<Chat>> GetChatsFromTableAsync()
        {
            var chats = new List<Chat>();

            await foreach (var entity in chatsTable.QueryAsync<ChatEntity>(filter: x => true))
            {
                var chat = new Chat(entity);
                chats.Add(chat);
            }

            return chats;
        }

        private async void ChatsTableThread()
        {
            while (true)
            {
                using (var transaction = StateManager.CreateTransaction())
                {
                    var temp = (await chatsDictionary.CreateEnumerableAsync(transaction)).GetAsyncEnumerator();
                    while (await temp.MoveNextAsync(CancellationToken.None))
                    {
                        var chat = temp.Current.Value;
                        var chatEntity = new ChatEntity(chat);
                        await chatsTable.UpsertEntityAsync(chatEntity, TableUpdateMode.Merge, CancellationToken.None);
                    }
                }
                Thread.Sleep(5000);
            }
        }

        public async Task PostMessageAsync(ChatMessageDto messageDto)
        {
            var chat = new Chat
            {
                Id = Guid.NewGuid().ToString(),
                SenderId = messageDto.SenderId,
                ReceiverId = messageDto.ReceiverId,
                Message = messageDto.Message,
                SentAt = messageDto.SentAt
            };

            using (var transaction = StateManager.CreateTransaction())
            {
                await chatsDictionary.AddAsync(transaction, chat.Id, chat);
                await transaction.CommitAsync();
            }

            var chatEntity = new ChatEntity(chat);
            await chatsTable.UpsertEntityAsync(chatEntity, TableUpdateMode.Merge);
        }

        public async Task<IEnumerable<ChatMessageDto>> GetMessageAsync(string senderId, string receiverId)
        {
            var messages = new List<ChatMessageDto>();
            using (var transaction = StateManager.CreateTransaction())
            {
                var enums = (await chatsDictionary.CreateEnumerableAsync(transaction)).GetAsyncEnumerator();
                while (await enums.MoveNextAsync(CancellationToken.None))
                {
                    var curr = enums.Current.Value;
                    if ((curr.SenderId == senderId && curr.ReceiverId == receiverId) || (curr.SenderId == receiverId && curr.ReceiverId == senderId))
                    {
                        messages.Add(new ChatMessageDto
                        {
                            SenderId = curr.SenderId,
                            ReceiverId = curr.ReceiverId,
                            Message = curr.Message,
                            SentAt = curr.SentAt.DateTime,
                        });
                    }
                }
            }
            var recentMessages = messages.OrderByDescending(m => m.SentAt).Take(10) .OrderBy(m => m.SentAt) .ToList();

            return recentMessages;
        }
    }
}
