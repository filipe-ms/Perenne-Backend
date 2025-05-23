using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using perenne.DTOs;
using perenne.Interfaces;
using perenne.Models;
using System.Security.Claims;

namespace perenne.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class FeedController : ControllerBase
    {
        private readonly IFeedService _feedService;
        private readonly IGroupService _groupService;

        public FeedController(IFeedService feedService, IGroupService groupService)
        {
            _feedService = feedService ?? throw new ArgumentNullException(nameof(feedService));
            _groupService = groupService ?? throw new ArgumentNullException(nameof(groupService));
        }

        // [host]/api/feed/{groupIdString}/post    
        [HttpPost("{groupIdString}/post")]
        public async Task<ActionResult<Post>> CreatePost(string groupIdString, [FromBody] PostDtos newPostDto)
        {
            var currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

            if (!Guid.TryParse(groupIdString, out var groupId))
                return BadRequest(new { message = "Invalid Group ID format in URL." });

            if (string.IsNullOrEmpty(currentUserIdString) || !Guid.TryParse(currentUserIdString, out var currentUserId))
                return Unauthorized(new { message = "Invalid or missing user ID." });

            var groupEntity = await _groupService.GetGroupByIdAsync(groupId);

            if (groupEntity == null)
                return NotFound(new { message = $"Group not found with ID: {groupId}" });

            if (groupEntity.Feed == null)
                return NotFound(new { message = $"Feed not found for Group ID: {groupId}. The group may not have an active feed." });

            var post = new Post
            {
                Title = newPostDto.Title,
                Content = newPostDto.Content,
                FeedId = groupEntity.Feed.Id,
                UserId = currentUserId,
                CreatedById = currentUserId,
                ImageUrl = newPostDto.ImageUrl,
                CreatedAt = DateTime.UtcNow,
            };

            var createdPost = await _feedService.CreatePostAsync(post);
            if (createdPost == null)
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while creating the post." });

            return Ok(createdPost);
        }

        // [host]/api/feed/{feedId}/GetLast{num}Posts
        [HttpGet("{feedIdString}/GetLast{num}Posts")]
        public async Task<ActionResult<IEnumerable<Post>>> GetLastXPosts(string feedIdString, int num)
        {
            if (!Guid.TryParse(feedIdString, out var feedId))
                return BadRequest(new { message = "Invalid Feed ID format." });

            if (num <= 0)
                return BadRequest(new { message = "Number of posts to retrieve must be positive." });

            var posts = await _feedService.GetLastXPostsAsync(feedId, num);

            return Ok(posts);
        }
    }
}
