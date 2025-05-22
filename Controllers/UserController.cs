using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using perenne.DTOs;
using perenne.Interfaces;
using perenne.Models;
using System.Security.Claims;

namespace perenne.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(IUserService _userService) : ControllerBase
{
    
    [HttpPost(nameof(Create))] // Criar usuário
    public async Task Create( [FromBody] UserRegisterDto dto) =>
        await _userService.RegisterUserAsync(dto);

    [HttpPost(nameof(Login))] // Login
    public async Task<User> Login( [FromBody] UserLoginDto dto)
    {
        var token = await _userService.LoginAsync(dto.Email, dto.Password);
        return token;
    }

    [Authorize]
    [HttpGet(nameof(GetGroups))] // Puxa os grupos de um usuário
    public async Task<ActionResult<IEnumerable<GroupSummaryDto>>> GetGroups()
    {
        // Isso aqui pega o user ID baseado nas claims do JWT
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(userIdString))
            return Unauthorized(new { message = $"[{nameof(GetGroups)}] Claim de User ID não encontrado na token." });

        if (!Guid.TryParse(userIdString, out var userIdGuid))
            return BadRequest(new { message = $"[{nameof(GetGroups)}] ID inválido registrado na token." });

        try
        {
            var groups = await _userService.GetGroupsByUserIdAsync(userIdGuid);

            if (groups == null || !groups.Any())
            {
                return Ok(Enumerable.Empty<GroupSummaryDto>());
            }

            var groupDtos = groups.Select(g => new GroupSummaryDto
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description,
                ChatChannelId = g.ChatChannel?.Id,
                MemberCount = g.Members?.Count ?? 0
            }).ToList();

            return Ok(groupDtos);
        }
        catch (KeyNotFoundException knfex)
        {
            return NotFound(new { message = knfex.Message });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{nameof(GetGroups)}] Erro encontrando os grupos: {ex}");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"[{nameof(GetGroups)}] Um erro ocorreu ao buscar os grupos do usuário." });
        }
    }

    [HttpGet("ping")]
    public ActionResult<string> TESTEPINGFRONT()
    {
        return "pong";
    }


    // PEIXOTO
}
