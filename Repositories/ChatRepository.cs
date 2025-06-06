using Microsoft.EntityFrameworkCore;
using perenne.Data;
using perenne.Interfaces;
using perenne.Models;

namespace perenne.Repositories
{
    public class ChatRepository(ApplicationDbContext context) : IChatRepository
    {
        private readonly ApplicationDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

        public async Task<ChatChannel> GetChatChannelByIdAsync(Guid chatChannelId)
        {
            return await _context.ChatChannels
                .Include(cc => cc.User1)
                .Include(cc => cc.User2)
                .Include(cc => cc.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
                .FirstOrDefaultAsync(cc => cc.Id == chatChannelId)
                ?? throw new KeyNotFoundException($"Chat channel with ID {chatChannelId} not found.");
        }

        public async Task<ChatChannel> CreateChatChannelAsync(ChatChannel chatChannel)
        {
            ArgumentNullException.ThrowIfNull(chatChannel);

            if (chatChannel.IsPrivate && chatChannel.User1Id.HasValue && chatChannel.User2Id.HasValue && chatChannel.User1Id.Value > chatChannel.User2Id.Value)
            {
                (chatChannel.User1Id, chatChannel.User2Id) = (chatChannel.User2Id, chatChannel.User1Id);
            }

            var c = await _context.ChatChannels.AddAsync(chatChannel);
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

        public async Task<IEnumerable<ChatMessage>> GetLastXMessagesAsync(Guid chatChannelId, int num)
        {
            var messages = await _context.ChatMessages
                .Where(m => m.ChatChannelId == chatChannelId)
                .OrderByDescending(m => m.CreatedAt)
                .Take(num)
                .OrderBy(m => m.CreatedAt)
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
                    .OrderBy(m => m.CreatedAt)
                    .ToListAsync();
                result.AddRange(messages);
            }
            return result;
        }

        public async Task<ChatChannel?> GetPrivateChatChannelAsync(Guid user1Id, Guid user2Id)
        {
            var id1 = user1Id < user2Id ? user1Id : user2Id;
            var id2 = user1Id < user2Id ? user2Id : user1Id;

            return await _context.ChatChannels
                .FirstOrDefaultAsync(cc => cc.IsPrivate &&
                                      ((cc.User1Id == id1 && cc.User2Id == id2) ||
                                       (cc.User1Id == id2 && cc.User2Id == id1)));
        }

        public async Task<IEnumerable<ChatChannel>> GetUserPrivateChatChannelsAsync(Guid userId)
        {
            return await _context.ChatChannels
                .Where(cc => cc.IsPrivate && (cc.User1Id == userId || cc.User2Id == userId))
                .Include(cc => cc.User1)
                .Include(cc => cc.User2)
                .Include(cc => cc.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
                .ToListAsync();
        }
    }
}