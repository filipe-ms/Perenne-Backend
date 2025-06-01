using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using perenne.DTOs;
using perenne.Interfaces;
using System.Security.Claims;

namespace perenne.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UserController(IUserService _userService) : ControllerBase
{
    // [host]/api/user/getgroups/
    [HttpGet(nameof(GetGroups))]
    public async Task<ActionResult<IEnumerable<GroupSummaryDto>>> GetGroups()
    {
        // Pega o user ID baseado nas claims do JWT
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        Guid userIdGuid = _userService.ParseUserId(userIdString);

        try
        {
            var groups = await _userService.GetGroupsByUserIdAsync(userIdGuid);

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
}
