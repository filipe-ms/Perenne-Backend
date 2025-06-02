using Microsoft.AspNetCore.Mvc;
using perenne.DTOs;
using perenne.Interfaces;
using perenne.Models;

namespace perenne.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GuestController(IGuestService _guestService) : ControllerBase
    {
        // [host]/api/guest/create/
        [HttpPost("create")]
        public async Task<bool> Create([FromBody] GuestRegisterDto dto)
        {
            var user = new User
            {
                Email = dto.Email,
                Password = dto.Password,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                CPF = dto.CPF,
                IsValidated = false,
                CreatedAt = DateTime.UtcNow
            };

            return await _guestService.CreateUserAsync(user);
        }
        
        // [host]/api/guest/login/
        [HttpPost("login")]
        public async Task<User> Login([FromBody] GuestLoginDto dto)
        {
            if (dto == null || dto.Email == null || dto.Password == null)
                throw new ArgumentNullException(nameof(dto));

            var user = await _guestService.UserLoginAsync(dto.Email, dto.Password);

            return user;
        }
    }
}
