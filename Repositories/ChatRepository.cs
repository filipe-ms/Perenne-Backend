using perenne.Data;
using perenne.Interfaces;

namespace perenne.Repositories
{
    public class ChatRepository(ApplicationDbContext context) : IChatRepository
    {

        public async Task<ChatChannel> AddChatChannelAsync(ChatChannel chat)
        {
            var c = await context.ChatChannels.AddAsync(chat);
            await context.SaveChangesAsync();
            return c.Entity;
        }
    }
    
}
