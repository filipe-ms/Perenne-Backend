using perenne.Models;

namespace perenne.Interfaces
{
    public interface IFeedService
    {
        Task<Feed> CreateFeedAsync(Feed feed);

        Task<IEnumerable<Post>> GetAllPostsByFeedIdAsync(Guid feedId);
        Task<IEnumerable<Post>> GetLastXPostsAsync(Guid feedId, int num);
        Task<bool> DeletePostAsync(Guid postId);

        // Post

        Task<Post> CreatePostAsync(Post post);
        Task<Post> UpdatePostAsync(Post post);
        Task<Post> GetPostByIdAsync(Guid postId);
    }
}
