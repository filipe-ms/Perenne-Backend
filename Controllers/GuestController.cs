using Microsoft.AspNetCore.Mvc;
using perenne.DTOs;
using perenne.FTOs;
using perenne.Interfaces;
using perenne.Models;

namespace perenne.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GuestController(IGuestService guestService) : ControllerBase
    {
        // [host]/api/guest/create/
        [HttpPost("create")]
        public async Task<bool> Create([FromBody] GuestRegisterDTO dto)
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

            return await guestService.CreateUserAsync(user);
        }
        
        // [host]/api/guest/login/
        [HttpPost("login")]
        public async Task<ProfileInfoFTO> Login([FromBody] GuestLoginDTO dto)
        {
            if (dto == null || dto.Email == null || dto.Password == null)
                throw new ArgumentNullException(nameof(dto));

            var user = await guestService.UserLoginAsync(dto.Email, dto.Password);
            var response = new ProfileInfoFTO(user);
            return response;
        }
    }
}
