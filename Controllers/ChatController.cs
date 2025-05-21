using Microsoft.AspNetCore.Mvc;
using perenne.Interfaces;
using perenne.Models;

namespace perenne.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController(IChatService _chatService) : ControllerBase
    {
        // Pega as últimas X mensagens
        [HttpGet("{chatid}/GetLast{num}Messages")]
        public async Task<IEnumerable<ChatMessage>> GetLastXMessages(Guid chatid, int num)
        {
            var messages = await _chatService.GetLastXMessagesAsync(chatid, num);
            return messages;
        }
    }
}
