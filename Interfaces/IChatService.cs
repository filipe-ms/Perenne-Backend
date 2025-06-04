using perenne.Models;
using perenne.Repositories;

namespace perenne.Interfaces
{
    public interface IChatService
    {
        Task<ChatChannel> GetChatChannelByIdAsync(Guid chatChannelId);
        Task<ChatChannel> CreateChatChannelAsync(ChatChannel channel);
        Task<ChatMessage> CreateChatMessageAsync(ChatMessage message);

        Task<IEnumerable<ChatMessage>> GetLastXMessagesAsync(Guid chatId, int num);

        // Chat privado

        Task<ChatChannel> GetOrCreatePrivateChatChannelAsync(Guid user1Id, Guid user2Id);
        Task<IEnumerable<ChatChannel>> GetUserPrivateChatChannelsAsync(Guid userId);
    }
}
