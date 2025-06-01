using perenne.Models;
using perenne.Repositories;

namespace perenne.Interfaces
{
    public interface IChatService
    {
        Task<ChatChannel> CreateChatChannelAsync(ChatChannel channel);
        Task<ChatMessage> CreateChatMessageAsync(ChatMessage message);

        Task<IEnumerable<ChatMessage>> GetLastXMessagesAsync(Guid chatId, int num);
    }
}
