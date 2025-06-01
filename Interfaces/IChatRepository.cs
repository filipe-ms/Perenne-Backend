using perenne.Models;

namespace perenne.Interfaces
{
    public interface IChatRepository
    {
        Task<ChatChannel> CreateChatChannelAsync(ChatChannel chat);
        Task<ChatMessage> CreateChatMessageAsync(ChatMessage message);
        Task<IEnumerable<ChatMessage>> GetLastXMessagesAsync(Guid chatid, int num);
    }
}
