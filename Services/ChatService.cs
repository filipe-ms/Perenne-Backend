using perenne.Interfaces;

namespace perenne.Services
{
    public class ChatService(IChatRepository chatRepository) : IChatService
    {
        public async Task<ChatChannel> CreateChatChannelAsync(ChatChannel channel)
        {
            var c = await chatRepository.AddChatChannelAsync(channel);
            return c;
        }
    }
}
