using Microsoft.EntityFrameworkCore;
using perenne.Data;
using perenne.Interfaces;
using perenne.Models;

namespace perenne.Repositories
{
    public class FeedRepository(ApplicationDbContext _context) : IFeedRepository
    {
        public async Task<Feed> CreateFeedAsync(Feed feed)
        {
            var f = await _context.Feed.AddAsync(feed);
            await _context.SaveChangesAsync();
            return f.Entity;
        }

        public async Task<Post?> GetPostByIdAsync(Guid id)
        {
            return await _context.Posts
                .Include(p => p.Feed)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Post>> GetAllPostsAsync()
        {
            return await _context.Posts
                .Include(p => p.Feed)
                .Include(p => p.User)
                .ToListAsync();
        }

        public async Task AddPostAsync(Post post)
        {
            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePostAsync(Post post)
        {
            _context.Posts.Update(post);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePostAsync(Guid id)
        {
            var post = await GetPostByIdAsync(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Post>> GetPostsByFeedIdAsync(Guid feedId)
        {
            return await _context.Posts
                .Where(p => p.FeedId == feedId)
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Post>> GetLastXPostsAsync(Guid feedId, int num)
        {
            if (num <= 0)
            {
                return Enumerable.Empty<Post>();
            }

            return await _context.Posts
                .Where(p => p.FeedId == feedId)
                .OrderByDescending(p => p.CreatedAt)
                .Take(num)
                .Include(p => p.User)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}