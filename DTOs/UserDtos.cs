using Microsoft.AspNetCore.Mvc;
using perenne.Models;

namespace perenne.DTOs
{
    public class UserLoginResponseDto
    {
        public User? User { get; set; }
        public string? Token { get; set; }
    }
}
