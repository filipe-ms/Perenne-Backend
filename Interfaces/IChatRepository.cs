using perenne.Models;

namespace perenne.Interfaces
{
    public interface IChatRepository
    {
        Task<ChatChannel> AddChatChannelAsync(ChatChannel chat);
        Task<ChatMessage> AddChatMessageAsync(ChatMessage message);
        Task<IEnumerable<ChatMessage>> GetLastXMessagesAsync(Guid chatid, int num);
    }
}
