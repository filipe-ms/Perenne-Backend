using perenne.Models;

namespace perenne.Interfaces
{
    public interface IChatService
    {
        Task<ChatChannel> CreateChatChannelAsync(ChatChannel channel);
        Task<ChatMessage> AddChatMessageAsync(ChatMessage message);
    }
}
