using Microsoft.AspNetCore.Mvc;
using perenne.Interfaces;
using perenne.Models;

[ApiController]
[Route("api/[controller]")]
public class FeedController(IFeedService _feedService) : ControllerBase
{
    // Pega os últimos X posts
    [HttpGet("{feedId}/GetLast{num}Posts")]
    public async Task<IEnumerable<Post>> GetLastXPosts(Guid feedId, int num)
    {
        var messages = await _feedService.GetLastXPostsAsync(feedId, num);
        return messages;
    }
}