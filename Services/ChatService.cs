using perenne.Interfaces;
using perenne.Models;

namespace perenne.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;

        public ChatService(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository ?? throw new ArgumentNullException(nameof(chatRepository));
        }

        public async Task<ChatChannel> CreateChatChannelAsync(ChatChannel channel)
        {
            if (channel == null)
            {
                throw new ArgumentNullException(nameof(channel));
            }
            var c = await _chatRepository.AddChatChannelAsync(channel);
            return c;
        }

        public async Task<ChatMessage> AddChatMessageAsync(ChatMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            var m = await _chatRepository.AddChatMessageAsync(message);
            return m;
        }
    }
}
