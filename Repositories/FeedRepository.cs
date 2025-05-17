using Microsoft.EntityFrameworkCore;
using perenne.Data;
using perenne.Interfaces;
using perenne.Models;

namespace perenne.Repositories
{
    public class FeedRepository(ApplicationDbContext context) : IFeedRepository
    {
        public async Task<Feed> CreateFeedAsync(Feed feed)
        {
            var f = await context.Feed.AddAsync(feed);
            await context.SaveChangesAsync();
            return f.Entity;
        }

        public async Task<Post?> GetPostByIdAsync(Guid id)
        {
            return await context.Posts
                .Include(p => p.Feed)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Post>> GetAllPostsAsync()
        {
            return await context.Posts
                .Include(p => p.Feed)
                .Include(p => p.User)
                .ToListAsync();
        }

        public async Task AddPostAsync(Post post)
        {
            await context.Posts.AddAsync(post);
            await context.SaveChangesAsync();
        }

        public async Task UpdatePostAsync(Post post)
        {
            context.Posts.Update(post);
            await context.SaveChangesAsync();
        }

        public async Task DeletePostAsync(Guid id)
        {
            var post = await GetPostByIdAsync(id);
            if (post != null)
            {
                context.Posts.Remove(post);
                await context.SaveChangesAsync();
            }
        }
    }
}