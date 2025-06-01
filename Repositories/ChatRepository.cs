using Microsoft.EntityFrameworkCore;
using perenne.Data;
using perenne.Interfaces;
using perenne.Models;

namespace perenne.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly ApplicationDbContext _context;

        public ChatRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<ChatChannel> CreateChatChannelAsync(ChatChannel chat)
        {
            var c = await _context.ChatChannels.AddAsync(chat);
            await _context.SaveChangesAsync();
            return c.Entity;
        }

        public async Task<ChatMessage> CreateChatMessageAsync(ChatMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

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
    }
}