using perenne.Models;

namespace perenne.Interfaces
{
    public interface IFeedRepository
    {
        Task<Feed> CreateFeedAsync(Feed feed);
        
        // Post
        Task<Post> CreatePostAsync(Post post);
        Task UpdatePostAsync(Post post);
        Task<bool> DeletePostAsync(Guid id);
        Task<Post> GetPostByIdAsync(Guid id);

        // Gets all
        Task<IEnumerable<Post>> GetPostsByFeedIdAsync(Guid feedId);

        // Gets X
        Task<IEnumerable<Post>> GetLastXPostsAsync(Guid feedId, int num);
    }
}