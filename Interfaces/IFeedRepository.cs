using perenne.Models;

namespace perenne.Interfaces
{
    public interface IFeedRepository
    {
        Task<Feed> CreateFeedAsync(Feed feed);
        
        // Post
        Task<Post> CreatePostAsync(Post post);
        Task<Post> UpdatePostAsync(Post post);
        Task<bool> DeletePostAsync(Guid id);
        Task<Post> GetPostByIdAsync(Guid id);

        // Gets all
        Task<IEnumerable<Post>> GetAllPostsByFeedIdAsync(Guid feedId);

        // Gets X
        Task<IEnumerable<Post>> GetLastXPostsAsync(Guid feedId, int num);
    }
}