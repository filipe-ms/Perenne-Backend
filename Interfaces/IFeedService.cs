namespace perenne.Interfaces
{
    public interface IFeedService
    {
        Task<Feed> CreateFeedAsync(Feed feed);
    }
}
