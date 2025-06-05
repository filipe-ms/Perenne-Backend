using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using perenne.DTOs;
using perenne.FTOs;
using perenne.Interfaces;
using System.Security.Claims;

namespace perenne.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UserController(IUserService userService, IGroupService groupService) : ControllerBase
{
    // [host]/api/user/getgroups/
    [HttpGet(nameof(GetGroups))]
    public async Task<ActionResult<IEnumerable<GroupSummaryDto>>> GetGroups()
    {
        // Pega o user ID baseado nas claims do JWT
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        Guid userIdGuid = userService.ParseUserId(userIdString);

        try
        {
            var groups = await userService.GetGroupsByUserIdAsync(userIdGuid);

            if (groups == null || !groups.Any()) return Ok(Enumerable.Empty<GroupSummaryDto>());

            var groupDtos = groups.Select(g => new GroupSummaryDto
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description!,
                MemberCount = g.Members?.Count ?? 0
            }).ToList();

            return Ok(groupDtos);
        }

        catch (Exception ex)
        {
            Console.WriteLine($"[{nameof(GetGroups)}] Um erro ocorreu ao buscar os grupos do usuário.\n\t->{ex}");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocorreu um erro inesperado ao processar sua solicitação. Tente novamente mais tarde." });
        }
    }

    // [host]/api/user/getuserinfo
    [HttpGet(nameof(GetUserInfo))]
    public async Task<ActionResult<ProfileInfoFTO>> GetUserInfo()
    {
        var userId = GetCurrentUserId();

        try
        {
            var user = await userService.GetUserByIdAsync(userId!.Value);
            var groups = await userService.GetGroupsByUserIdAsync(userId!.Value);
            var groupNames = groups.Select(g => g.Name).ToList() ?? [];

            var userProfileInfo = new ProfileInfoFTO(user)
            {
                Groups = groupNames
            };

            return Ok(userProfileInfo);
        }

        catch { throw; }
    }

    // GET [host]/api/user/getgroupjoinrequests
    [HttpGet(nameof(GetGroupJoinRequests))]
    public async Task<ActionResult<IEnumerable<object>>> GetGroupJoinRequests()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized(new { message = "Usuário não autenticado." });

        var requests = await groupService.GetPendingRequestsForUserAsync(userId!.Value);

        var response = requests.Select(r => new
        {
            RequestId = r.Id,
            r.GroupId,
            GroupName = r.Group?.Name,
            r.RequestedAt,
            Status = r.Status.ToString(),
            r.Message
        });
        return Ok(response);
    }

    // [host]/api/user/profile/{userIdString}
    [HttpGet("profile/{userIdString}")]
    public async Task<ActionResult<ProfileInfoFTO>> GetUserInfoById(string userIdString)
    {
        var user = await userService.GetUserByIdAsync(userService.ParseUserId(userIdString));
        if (user == null) return NotFound(new { message = $"Usuário com ID {userIdString} não encontrado." });
        var userFTO = new ProfileInfoFTO(user);
        return Ok(userFTO);
    }

    // [host]/api/user/getallusers
    [HttpGet(nameof(GetAllUsers))]
    public async Task<ActionResult<IEnumerable<ProfileInfoFTO>>> GetAllUsers()
    {
        var users = await userService.GetAllUsersAsync();
        if (users == null || !users.Any()) return Ok(Enumerable.Empty<ProfileInfoFTO>());
        var userDtos = users.Select(u => new ProfileInfoFTO(u)).ToList();
        return Ok(userDtos);
    }

    // Utils
    private Guid? GetCurrentUserId()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return userService.ParseUserId(userIdString);
    }
}


