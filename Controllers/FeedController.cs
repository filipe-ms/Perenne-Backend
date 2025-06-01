using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using perenne.DTOs;
using perenne.FTOs;
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
        [HttpPost("{groupIdString}/createpost")]
        public async Task<ActionResult<PostFto>> CreatePost(string groupIdString, [FromBody] PostDto newPostDto)
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
                Title = newPostDto.Title!,
                Content = newPostDto.Content!,
                FeedId = groupEntity.Feed.Id,
                UserId = currentUserId,
                CreatedById = currentUserId,
                ImageUrl = newPostDto.ImageUrl,
                CreatedAt = DateTime.UtcNow,
            };

            var createdPost = await _feedService.CreatePostAsync(post);
            if (createdPost == null)
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while creating the post." });

            var answer = new PostFto
            {
                Title = createdPost.Title,
                Content = createdPost.Content,
                Creator = createdPost.UserId,
                ImageUrl = createdPost.ImageUrl,
                CreatedAt = createdPost.CreatedAt
            };

            return Ok(answer);
        }

        // [host]/api/deletepost/{postIdString}
        [HttpDelete("{groupIdString}/deletepost/{postIdString}")]
        public async Task<IActionResult> DeletePost(string postIdString)
        {
            if (!Guid.TryParse(postIdString, out var postId))
                return BadRequest(new { result = false, message = "Invalid Post ID format." });

            var result = await _feedService.DeletePostAsync(postId);
            return Ok(result);   
        }

        // [host]/api/feed/{groupIdString}/getposts/{num}
        [HttpGet("{groupIdString}/getposts/{num}")]
        public async Task<ActionResult<IEnumerable<PostFto>>> GetLastXPosts(string groupIdString, int num)
        {
            if (!Guid.TryParse(groupIdString, out var groupId)) return BadRequest(new { message = "Invalid Feed ID format." });
            if (num <= 0) return BadRequest(new { message = "Number of posts to retrieve must be positive." });

            var group = await _groupService.GetGroupByIdAsync(groupId);
            var feedId = group.Feed!.Id;
            var posts = await _feedService.GetLastXPostsAsync(feedId, num);

            var answer = posts.Select((Post x) =>
            new PostFto()
            {
                Id = x.Id,
                Title = x.Title,
                Content = x.Content,
                ImageUrl = x.ImageUrl,
                Creator = (Guid)x.CreatedById!,
                CreatedAt = x.CreatedAt
            });

            return Ok(answer);
        }
    }
}
