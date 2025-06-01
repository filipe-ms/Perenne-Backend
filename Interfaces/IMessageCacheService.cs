using perenne.Models;

namespace perenne.Interfaces
{
    public interface IMessageCacheService
    {
        Task InitMessageCacheServiceAsync();
        Task<IEnumerable<ChatMessage>> GetMessagesByChatChannelIdAsync(Guid groupId);
        Task<ChatMessage> HandleChatMessageReceived(ChatMessage message);
    }
}
