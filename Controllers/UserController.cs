using Microsoft.AspNetCore.Mvc;
using perenne.DTOs;
using perenne.Services;

namespace perenne.Controllers;

[ApiController]
[Route("user")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] UserRegisterDto dto)
    {
        try
        {
            var result = await _userService.RegisterUserAsync(dto);

            if (!result) return BadRequest("User with this email or CPF already exists.");

            return Ok("User created successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"User registration failed: {ex.Message}");
            return StatusCode(500, "An error occurred while creating the user.");
        }
    }
}
