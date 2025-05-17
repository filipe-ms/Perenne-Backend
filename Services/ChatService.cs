using perenne.Interfaces;

namespace perenne.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;

        public ChatService(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository;
        }

        public async Task<ChatChannel> CreateChatChannelAsync(ChatChannel channel)
        {
            var c = await _chatRepository.AddChatChannelAsync(channel);
            return c;
        }
    }
}
