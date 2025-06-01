using Microsoft.AspNetCore.Mvc;
using perenne.Interfaces;
using perenne.Models;

namespace perenne.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IGroupService _groupService;

        public ChatController(IChatService chatService, IGroupService groupService)
        {
            _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));
            _groupService = groupService ?? throw new ArgumentNullException(nameof(groupService));
        }

        // Pega as últimas X mensagens
        [HttpGet("{groupIdString}/getmessages/{num}")]
        public async Task<ActionResult<IEnumerable<ChatMessage>>> GetLastXMessages(string groupIdString, int num)
        {
            if(!Guid.TryParse(groupIdString, out Guid groupId)) return BadRequest(new { message = "Id de Grupo inválido na URL." });
            if (num <= 0) return BadRequest(new { message = "Número de posts deve ser positivo." });

            var group = await _groupService.GetGroupByIdAsync(groupId);
            var chatId = group.ChatChannel!.Id;
            var messages = await _chatService.GetLastXMessagesAsync(chatId, num);
            
            return Ok(messages);
        }
    }
}
