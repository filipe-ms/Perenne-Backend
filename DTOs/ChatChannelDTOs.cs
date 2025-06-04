using System.ComponentModel.DataAnnotations;

namespace perenne.DTOs
{
    public class StartPrivateChatRequest
    {
        [Required]
        public string RecipientUserId { get; set; } = string.Empty;
    }
}
