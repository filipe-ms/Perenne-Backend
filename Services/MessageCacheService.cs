using perenne.Interfaces;
using perenne.Models;
using System.Collections.Concurrent;

namespace perenne.Services
{
    public class MessageCache
    {
        public ConcurrentDictionary<Guid, ConcurrentQueue<ChatMessage>> Messages { get; } = new();
        public bool IsInitialized { get; set; } = false;
    }

    public class MessageCacheService(
        MessageCache messageCache,
        IChatRepository chatRepository,
        IChatService chatService) : IMessageCacheService
    {
        private readonly MessageCache _messageCache = messageCache;
        private readonly IChatRepository _chatRepository = chatRepository ?? throw new ArgumentNullException(nameof(chatRepository));
        private readonly IChatService _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));

        public async Task InitMessageCacheServiceAsync()
        {
            if (_messageCache.IsInitialized) return;

            var messages = await _chatRepository.RetrieveChatMessageHistoryForCache();

            var grouped = messages
                .GroupBy(m => m.ChatChannelId)
                .ToDictionary(
                    g => g.Key,
                    g => new ConcurrentQueue<ChatMessage>(g.OrderBy(m => m.CreatedAt))
                );

            foreach (var pair in grouped)
            {
                _messageCache.Messages.TryAdd(pair.Key, pair.Value);
            }

            _messageCache.IsInitialized = true;
        }

        public async Task<IEnumerable<ChatMessage>> GetMessagesByChatChannelIdAsync(Guid chatId)
        {
            if (!_messageCache.IsInitialized)
                throw new InvalidOperationException("O cache ainda não foi inicializado. Por favor aguarde.");

            if (_messageCache.Messages.TryGetValue(chatId, out var messages))
            {
                return messages.ToList();
            }

            var retrievedMessages = await _chatRepository.GetLastXMessagesAsync(chatId, 100);
            return retrievedMessages;
        }

        public async Task<ChatMessage> HandleChatMessageReceived(ChatMessage message)
        {
            ArgumentNullException.ThrowIfNull(message);
            var createdMessage = await _chatService.CreateChatMessageAsync(message);

            if (_messageCache.IsInitialized)
            {
                var queue = _messageCache.Messages.GetOrAdd(createdMessage.ChatChannelId, _ => new ConcurrentQueue<ChatMessage>());
                queue.Enqueue(createdMessage);
                while (queue.Count > 100)
                {
                    queue.TryDequeue(out _);
                }
            }

            return createdMessage;
        }
    }
}
