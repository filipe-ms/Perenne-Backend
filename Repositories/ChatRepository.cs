using Microsoft.EntityFrameworkCore;
using perenne.Data;
using perenne.Interfaces;
using perenne.Models;

namespace perenne.Repositories
{
    public class ChatRepository(ApplicationDbContext context) : IChatRepository
    {
        private readonly ApplicationDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

        // Funções do chat

        public async Task<ChatChannel> CreateChatChannelAsync(ChatChannel chat)
        {
            var c = await _context.ChatChannels.AddAsync(chat);
            await _context.SaveChangesAsync();
            return c.Entity;
        }

        public async Task<ChatMessage> CreateChatMessageAsync(ChatMessage message)
        {
            ArgumentNullException.ThrowIfNull(message);

            var m = await _context.ChatMessages.AddAsync(message);
            await _context.SaveChangesAsync();
            return m.Entity;
        }

        public async Task<IEnumerable<ChatMessage>> GetLastXMessagesAsync(Guid chatId, int num)
        {
            var messages = await _context.ChatMessages
                .Where(m => m.ChatChannelId == chatId)
                .OrderByDescending(m => m.CreatedAt)
                .Take(num)
                .ToListAsync();

            return messages;
        }

        // Funções do cache

        public async Task<IEnumerable<ChatMessage>> RetrieveChatMessageHistoryForCache()
        {
            var channelIds = await _context.ChatChannels
                .Select(c => c.Id)
                .ToListAsync();

            var result = new List<ChatMessage>();

            foreach (var channelId in channelIds)
            {
                var messages = await _context.ChatMessages
                    .Where(m => m.ChatChannelId == channelId)
                    .OrderByDescending(m => m.CreatedAt)
                    .Take(100)
                    .ToListAsync();

                result.AddRange(messages);
            }

            return result;
        }
    }
}