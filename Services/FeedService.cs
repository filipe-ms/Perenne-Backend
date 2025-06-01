using perenne.Interfaces;
using perenne.Models;

namespace perenne.Services
{
    public class FeedService(IFeedRepository _feedRepository) : IFeedService
    {
        public async Task<Feed> CreateFeedAsync(Feed feed)
        {
            if (feed == null) throw new ArgumentNullException(nameof(feed));
            var f = await _feedRepository.CreateFeedAsync(feed);
            return f;
        }

        public async Task<Post> CreatePostAsync(Post post) =>
            await _feedRepository.CreatePostAsync(post);

        public async Task<IEnumerable<Post>> GetLastXPostsAsync(Guid feedId, int num)
        {
            if (num <= 0) return Enumerable.Empty<Post>();
            return await _feedRepository.GetLastXPostsAsync(feedId, num);
        }

        public async Task<bool> DeletePostAsync(Guid postId)
        {
            if (postId == Guid.Empty) throw new ArgumentException("Post ID cannot be empty.", nameof(postId));
            var result = await _feedRepository.DeletePostAsync(postId);
            return result;
        }
    }
}
