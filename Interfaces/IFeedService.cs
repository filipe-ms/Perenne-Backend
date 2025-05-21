using perenne.Models;

namespace perenne.Interfaces
{
    public interface IFeedService
    {
        Task<Feed> CreateFeedAsync(Feed feed);
        Task<IEnumerable<Post>> GetLastXPostsAsync(Guid feedId, int num);
    }
}
