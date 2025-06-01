using perenne.Interfaces;
using perenne.Models;

namespace perenne.Services
{
    public class ChatService(IChatRepository chatRepository) : IChatService
    {
        private readonly IChatRepository _chatRepository = chatRepository ?? throw new ArgumentNullException(nameof(chatRepository));

        public async Task<ChatChannel> CreateChatChannelAsync(ChatChannel channel)
        {
            ArgumentNullException.ThrowIfNull(channel);
            var c = await _chatRepository.CreateChatChannelAsync(channel);
            return c;
        }
        public async Task<ChatMessage> CreateChatMessageAsync(ChatMessage message)
        {
            ArgumentNullException.ThrowIfNull(message);

            var m = await _chatRepository.CreateChatMessageAsync(message);
            return m;
        }
        public async Task<IEnumerable<ChatMessage>> GetLastXMessagesAsync(Guid chatId, int num)
        {
            var messages = await _chatRepository.GetLastXMessagesAsync(chatId, num);
            return messages;
        }
    }
}
