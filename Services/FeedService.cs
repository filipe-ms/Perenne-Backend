using perenne.Interfaces;
using perenne.Models;
using perenne.Repositories;

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

        public async Task<IEnumerable<Post>> GetLastXPostsAsync(Guid feedId, int num)
        {
            if (num <= 0)
            {
                return Enumerable.Empty<Post>();
            }
            return await _feedRepository.GetLastXPostsAsync(feedId, num);
        }
    }
}
