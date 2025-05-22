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
                throw new ArgumentNullException(nameof(message));

            var m = await _chatRepository.AddChatMessageAsync(message);
            return m;
        }

        public async Task<IEnumerable<ChatMessage>> GetLastXMessagesAsync(Guid chatid, int num)
        {
            if (num <= 0)
                throw new ArgumentOutOfRangeException(nameof(num), "Número de mensagens deve ser maior que 0.");

            var messages = await _chatRepository.GetLastXMessagesAsync(chatid, num);
            return messages;
        }
    }
}
