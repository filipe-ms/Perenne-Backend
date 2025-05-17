using perenne.Interfaces;

namespace perenne.Services
{
    public class FeedService(IFeedRepository feedRepository) : IFeedService
    {
        public async Task<Feed> CreateFeedAsync(Feed feed)
        {
            var f = await feedRepository.CreateFeedAsync(feed);
            return f;
        }
    }
}
