using perenne.Interfaces;

namespace perenne.Services
{
    public class FeedService : IFeedService
    {
        private readonly IFeedRepository _feedRepository;

        public FeedService(IFeedRepository chatRepository)
        {
            _feedRepository = chatRepository;
        }
        public async Task<Feed> CreateFeedAsync(Feed feed)
        {
            var f = await _feedRepository.CreateFeedAsync(feed);
            return f;
        }
    }
}
