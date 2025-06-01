using perenne.Interfaces;
using perenne.Models;
using System.Collections.Concurrent;

namespace perenne.Services
{
    public class MessageCacheService(IChatRepository chatRepository, IChatService chatService) : IMessageCacheService
    {
        private readonly IChatRepository _chatRepository = chatRepository ?? throw new ArgumentNullException(nameof(chatRepository));
        private readonly IChatService _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));

        private ConcurrentDictionary<Guid, ConcurrentQueue<ChatMessage>>? _messageCache;

        public async Task InitMessageCacheServiceAsync()
        {
            var messages = await _chatRepository.RetrieveChatMessageHistoryForCache();

            var grouped = messages
                .GroupBy(m => m.ChatChannelId)
                .ToDictionary(
                    g => g.Key,
                    g => new ConcurrentQueue<ChatMessage>(g.OrderBy(m => m.CreatedAt)) // mantém a ordem temporal
                );

            _messageCache = new ConcurrentDictionary<Guid, ConcurrentQueue<ChatMessage>>(grouped);
        }

        public async Task<IEnumerable<ChatMessage>> GetMessagesByChatChannelIdAsync(Guid chatId)
        {
            if (_messageCache == null)
                throw new InvalidOperationException("O cache ainda não foi inicializado. Por favor aguarde.");

            if (_messageCache.TryGetValue(chatId, out var messages))
            {
                return messages.ToList();
            }

            var retrievedMessages = await _chatRepository.GetLastXMessagesAsync(chatId, 100);
            return retrievedMessages;
        }

        public async Task<ChatMessage> HandleChatMessageReceived(ChatMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            var createdMessage = await _chatService.CreateChatMessageAsync(message);

            if (_messageCache != null)
            {
                var queue = _messageCache.GetOrAdd(createdMessage.ChatChannelId, _ => new ConcurrentQueue<ChatMessage>());
                queue.Enqueue(createdMessage);
                
                while (queue.Count > 100) queue.TryDequeue(out _);
            }

            return createdMessage;
        }
    }
}
