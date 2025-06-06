using perenne.Interfaces;
using perenne.Models;

namespace perenne.Services
{
    public class ChatService(IChatRepository chatRepository) : IChatService
    {
        public async Task<ChatChannel> GetChatChannelByIdAsync(Guid chatChannelId)
        {
            var channel = await chatRepository.GetChatChannelByIdAsync(chatChannelId);
            return channel ?? throw new KeyNotFoundException($"Chat channel with ID {chatChannelId} not found.");
        }
        public async Task<ChatChannel> CreateChatChannelAsync(ChatChannel channel)
        {
            ArgumentNullException.ThrowIfNull(channel);
            var c = await chatRepository.CreateChatChannelAsync(channel);
            return c;
        }
        public async Task<ChatMessage> CreateChatMessageAsync(ChatMessage message)
        {
            ArgumentNullException.ThrowIfNull(message);

            var m = await chatRepository.CreateChatMessageAsync(message);
            return m;
        }
        public async Task<IEnumerable<ChatMessage>> GetLastXMessagesAsync(Guid chatId, int num)
        {
            var messages = await chatRepository.GetLastXMessagesAsync(chatId, num);
            return messages;
        }
        public async Task<ChatChannel> GetOrCreatePrivateChatChannelAsync(Guid user1Id, Guid user2Id)
        {
            if (user1Id == user2Id)
            {
                throw new ArgumentException("Não é possível criar um chat privado consigo mesmo.");
            }

            var u1 = user1Id < user2Id ? user1Id : user2Id;
            var u2 = user1Id < user2Id ? user2Id : user1Id;

            var existingChannel = await chatRepository.GetPrivateChatChannelAsync(u1, u2);
            if (existingChannel != null)
            {
                return existingChannel;
            }

            var newChannel = new ChatChannel
            {
                IsPrivate = true,
                User1Id = u1,
                User2Id = u2,
                GroupId = null
            };
            return await CreateChatChannelAsync(newChannel);
        }
        public async Task<IEnumerable<ChatChannel>> GetUserPrivateChatChannelsAsync(Guid userId)
        {
            return await chatRepository.GetUserPrivateChatChannelsAsync(userId);
        }

        public async Task<ChatChannel> GetPrivateChatChannelAsync(Guid user1Id, Guid user2Id)
        {
            if (user1Id == user2Id)
            {
                throw new ArgumentException("Não é possível obter um chat privado consigo mesmo.");
            }
            var u1 = user1Id < user2Id ? user1Id : user2Id;
            var u2 = user1Id < user2Id ? user2Id : user1Id;
            var channel = await chatRepository.GetPrivateChatChannelAsync(u1, u2);
            return channel ?? throw new KeyNotFoundException($"Chat privado entre {u1} e {u2} não encontrado.");
        }
    }
}
