using perenne.Models;

namespace perenne.Interfaces
{
    public interface IChatRepository
    {
        Task<ChatChannel> GetChatChannelByIdAsync(Guid chatChannelId);
        Task<ChatChannel> CreateChatChannelAsync(ChatChannel chat);
        Task<ChatMessage> CreateChatMessageAsync(ChatMessage message);
        Task<IEnumerable<ChatMessage>> GetLastXMessagesAsync(Guid chatid, int num);
        Task<IEnumerable<ChatMessage>> RetrieveChatMessageHistoryForCache();
        Task<ChatChannel?> GetPrivateChatChannelAsync(Guid user1Id, Guid user2Id);
        Task<IEnumerable<ChatChannel>> GetUserPrivateChatChannelsAsync(Guid userId);
    }
}
