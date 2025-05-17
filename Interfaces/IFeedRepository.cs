using perenne.Models;

namespace perenne.Interfaces
{
    public interface IFeedRepository
    {
        Task<Feed> CreateFeedAsync(Feed feed);
        Task<Post?> GetPostByIdAsync(Guid id);
        Task<IEnumerable<Post>> GetAllPostsAsync();
        Task AddPostAsync(Post post);
        Task UpdatePostAsync(Post post);
        Task DeletePostAsync(Guid id);
    }
}