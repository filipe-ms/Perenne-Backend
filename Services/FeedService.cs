using perenne.Interfaces;
using perenne.Models;

namespace perenne.Services
{
    public class FeedService(IFeedRepository feedRepository) : IFeedService
    {
        public async Task<Feed> CreateFeedAsync(Feed feed)
        {
            ArgumentNullException.ThrowIfNull(feed);
            var f = await feedRepository.CreateFeedAsync(feed);
            return f;
        }

        public async Task<Post> CreatePostAsync(Post post)
        {
            return await feedRepository.CreatePostAsync(post);
        }

        public async Task<IEnumerable<Post>> GetAllPostsByFeedIdAsync(Guid feedId)
        {
            if (feedId == Guid.Empty) throw new ArgumentException("Feed ID cannot be empty.", nameof(feedId));
            return await feedRepository.GetAllPostsByFeedIdAsync(feedId);
        }

        public async Task<IEnumerable<Post>> GetLastXPostsAsync(Guid feedId, int num)
        {
            if (num <= 0) return [];
            return await feedRepository.GetLastXPostsAsync(feedId, num);
        }

        public async Task<bool> DeletePostAsync(Guid postId)
        {
            if (postId == Guid.Empty) throw new ArgumentException("Post ID cannot be empty.", nameof(postId));
            var result = await feedRepository.DeletePostAsync(postId);
            return result;
        }
    }
}
