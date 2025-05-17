using Microsoft.AspNetCore.Mvc;
using perenne.DTOs;
using perenne.Models;
using perenne.Services;

namespace perenne.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(IUserService userService) : ControllerBase
{
    [HttpPost(nameof(Create))]
    public async Task Create(
        [FromBody] UserRegisterDto dto) =>
        await userService.RegisterUserAsync(dto);

    [HttpPost(nameof(Login))]
    public async Task<User> Login(
        [FromBody] UserLoginDto dto)
        {
            var token = await userService.LoginAsync(dto.Email, dto.Password);
            return token;
        }
}
