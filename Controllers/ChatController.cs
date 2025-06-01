using Microsoft.AspNetCore.Mvc;
using perenne.Interfaces;
using perenne.Models;

namespace perenne.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController(IMessageCacheService messageCacheService, IChatService chatService, IGroupService groupService) : ControllerBase
    {
        private readonly IMessageCacheService _messageCacheService = messageCacheService ?? throw new ArgumentNullException(nameof(messageCacheService));
        private readonly IChatService _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));
        private readonly IGroupService _groupService = groupService ?? throw new ArgumentNullException(nameof(groupService));

        // Pega as mensagens em cache
        [HttpGet("{groupIdString}/getcachedmessages")]
        public async Task<ActionResult<IEnumerable<ChatMessage>>> GetCachedMessages(string groupIdString)
        {
            if (!Guid.TryParse(groupIdString, out Guid groupId)) return BadRequest(new { message = "Id de Grupo inválido na URL." });
            var group = await _groupService.GetGroupByIdAsync(groupId);
            if (group == null) return NotFound(new { message = "Grupo não encontrado." });
            var chatId = group.ChatChannel!.Id;
            var messages = await _messageCacheService.GetMessagesByChatChannelIdAsync(chatId);
            return Ok(messages);
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
