using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using perenne.DTOs;
using perenne.FTOs;
using perenne.Interfaces;
using perenne.Models;
using System.Security.Claims;


namespace perenne.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FeedController(IFeedService feedService, IGroupService groupService, IUserService userService) : ControllerBase
    {
        // [host]/api/feed/{groupIdString}/post
        [HttpPost("{groupIdString}/createpost")]
        public async Task<ActionResult<PostFTO>> CreatePost(string groupIdString, [FromBody] PostDto newPostDto)
        {
            var user = await GetCurrentUser();
            var groupId = groupService.ParseGroupId(groupIdString);
            var membership = await groupService.GetGroupMemberAsync(user.Id, groupId);

            if (user.SystemRole != SystemRole.Admin &&
                user.SystemRole != SystemRole.SuperAdmin &&
                membership.Role != GroupRole.Coordinator &&
                membership.Role != GroupRole.Contributor)
                return Unauthorized(new { message = "O membro não tem permissão para fazer postagens." });


            var groupEntity = await groupService.GetGroupByIdAsync(groupId);

            if (groupEntity == null)
                return NotFound(new { message = $"Group not found with ID: {groupId}" });

            if (groupEntity.Feed == null)
                return NotFound(new { message = $"Feed not found for Group ID: {groupId}. The group may not have an active feed." });

            var post = new Post
            {
                Title = newPostDto.Title!,
                Content = newPostDto.Content!,
                FeedId = groupEntity.Feed.Id,
                UserId = user.Id,
                CreatedById = user.Id,
                ImageUrl = newPostDto.ImageUrl
            };

            var createdPost = await feedService.CreatePostAsync(post);
            if (createdPost == null)
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while creating the post." });

            var answer = new PostFTO(createdPost);

            return Ok(answer);
        }

        // [host]/api/deletepost/{postIdString}
        [HttpDelete("{groupIdString}/deletepost/{postIdString}")]
        public async Task<IActionResult> DeletePost(string groupIdString, string postIdString)
        {
            var user = await GetCurrentUser();
            var groupId = groupService.ParseGroupId(groupIdString);
            var membership = await groupService.GetGroupMemberAsync(user.Id, groupId);

            if (user.SystemRole != SystemRole.Admin &&
                user.SystemRole != SystemRole.SuperAdmin &&
                membership.Role != GroupRole.Coordinator &&
                membership.Role != GroupRole.Contributor)
                return Unauthorized(new { message = "O membro não tem permissão para apagar postagens." });


            if (!Guid.TryParse(postIdString, out var postId))
                return BadRequest(new { result = false, message = "Formato inválido." });

            var result = await feedService.DeletePostAsync(postId);
            return Ok(result);   
        }

        // [host]/api/feed/{groupIdString}/getallposts
        [HttpGet("{groupIdString}/getallposts")]
        public async Task<ActionResult<IEnumerable<PostFTO>>> GetAllPosts(string groupIdString)
        {
            if (!Guid.TryParse(groupIdString, out var groupId))
                return BadRequest(new { message = "Feed ID inválido." });
            var group = await groupService.GetGroupByIdAsync(groupId); 
            if (group == null) return NotFound(new { message = "Grupo não encontrado." });
            if (group.Feed == null) return NotFound(new { message = "Feed não associado ao grupo." });
            var posts = await feedService.GetAllPostsByFeedIdAsync(group.Feed.Id);
            var answer = posts.Select(x => new PostFTO(x)).ToList();
            return Ok(answer);
        }


        // [host]/api/feed/{groupIdString}/getposts/{num}
        [HttpGet("{groupIdString}/getposts/{num}")]
        public async Task<ActionResult<IEnumerable<PostFTO>>> GetLastXPosts(string groupIdString, int num)
        {
            if (!Guid.TryParse(groupIdString, out var groupId))
                return BadRequest(new { message = "Feed ID inválido." });

            if (num <= 0) return BadRequest(new { message = "Número de posts deve ser positivo." });

            var group = await groupService.GetGroupByIdAsync(groupId);
            if (group == null) return NotFound(new { message = "Grupo não encontrado." });

            if (group.Feed == null) return NotFound(new { message = "Feed não associado ao grupo." });

            var posts = await feedService.GetLastXPostsAsync(group.Feed.Id, num);

            var answer = posts.Select(x => new PostFTO(x)).ToList();

            return Ok(answer);
        }


        // Utils

        private Guid? GetCurrentUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return userService.ParseUserId(userIdString);
        }

        private async Task<User> GetCurrentUser()
        {
            var userId = GetCurrentUserId() ?? throw new UnauthorizedAccessException("Usuário não autenticado.");
            var user = await userService.GetUserByIdAsync(userId);
            return user ?? throw new Exception($"Usuário com ID {userId} não encontrado.");
        }

    }
}
