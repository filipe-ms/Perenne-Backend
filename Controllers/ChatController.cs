using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using perenne.Interfaces;
using perenne.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace perenne.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController(
        IMessageCacheService messageCacheService,
        IChatService chatService,
        IGroupService groupService,
        IUserService userService) : ControllerBase
    {
        // Pega as mensagens em cache
        [HttpGet("{groupIdString}/getcachedmessages")]
        public async Task<ActionResult<IEnumerable<ChatMessage>>> GetCachedMessages(string groupIdString)
        {
            if (!Guid.TryParse(groupIdString, out Guid groupId)) return BadRequest(new { message = "Id de Grupo inválido na URL." });
            var group = await groupService.GetGroupByIdAsync(groupId);
            if (group == null) return NotFound(new { message = "Grupo não encontrado." });
            var chatId = group.ChatChannel!.Id;
            var messages = await messageCacheService.GetMessagesByChatChannelIdAsync(chatId);
            return Ok(messages);
        }

        // Pega as últimas X mensagens
        [HttpGet("{groupIdString}/getmessages/{num}")]
        public async Task<ActionResult<IEnumerable<ChatMessage>>> GetLastXMessages(string groupIdString, int num)
        {
            if (!Guid.TryParse(groupIdString, out Guid groupId)) return BadRequest(new { message = "Id de Grupo inválido na URL." });
            if (num <= 0) return BadRequest(new { message = "Número de posts deve ser positivo." });

            var group = await groupService.GetGroupByIdAsync(groupId);
            var chatId = group.ChatChannel!.Id;
            var messages = await chatService.GetLastXMessagesAsync(chatId, num);

            return Ok(messages);
        }

        [HttpPost("private/start")]
        public async Task<ActionResult<object>> StartOrGetPrivateChat([FromBody] StartPrivateChatRequest request)
        {
            var senderUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(senderUserIdString) || !Guid.TryParse(senderUserIdString, out Guid senderUserId))
            {
                return Unauthorized(new { message = "Usuário não autenticado ou ID inválido." });
            }

            if (!Guid.TryParse(request.RecipientUserId, out Guid recipientUserId))
            {
                return BadRequest(new { message = "ID do destinatário inválido." });
            }

            if (senderUserId == recipientUserId)
            {
                return BadRequest(new { message = "Não é possível iniciar um chat consigo mesmo." });
            }

            try
            {
                var chatChannel = await chatService.GetOrCreatePrivateChatChannelAsync(senderUserId, recipientUserId);
                var recipientUser = await userService.GetUserByIdAsync(recipientUserId);

                return Ok(new
                {
                    chatChannel.Id,
                    chatChannel.IsPrivate,
                    User1Id = chatChannel.User1Id,
                    User2Id = chatChannel.User2Id,
                    OtherParticipant = new // Informações do outro usuário na conversa
                    {
                        Id = recipientUser?.Id.ToString(), // O ID do outro usuário
                        DisplayName = recipientUser != null ? $"{recipientUser.FirstName} {recipientUser.LastName}" : "Usuário Desconhecido"
                        // Adicione outros campos do usuário conforme necessário
                    }
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Logar o erro
                return StatusCode(500, new { message = "Ocorreu um erro ao iniciar o chat privado.", details = ex.Message });
            }
        }

        [HttpGet("private/channels")]
        public async Task<ActionResult<IEnumerable<object>>> GetMyPrivateChatChannels()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized(new { message = "Usuário não autenticado ou ID inválido." });
            }

            var channels = await chatService.GetUserPrivateChatChannelsAsync(userId);
            var result = channels.Select(async cc =>
            {
                // Determina quem é o "outro" usuário na conversa
                var otherUserId = cc.User1Id == userId ? cc.User2Id : cc.User1Id;
                User? otherUser = null;
                if (otherUserId.HasValue)
                {
                    otherUser = await userService.GetUserByIdAsync(otherUserId.Value);
                }

                return new
                {
                    cc.Id,
                    cc.IsPrivate,
                    OtherParticipant = new
                    {
                        Id = otherUser?.Id.ToString(),
                        DisplayName = otherUser != null ? $"{otherUser.FirstName} {otherUser.LastName}" : "Usuário Desconhecido",
                    },
                    LastMessage = cc.Messages.FirstOrDefault() != null ? new
                    {
                        Text = cc.Messages.First().Message,
                        CreatedAt = cc.Messages.First().CreatedAt,
                        SenderId = cc.Messages.First().CreatedById
                    } : null
                };
            });

            return Ok(await Task.WhenAll(result));
        }

        [HttpGet("private/{chatChannelIdString}/messages/{num}")]
        public async Task<ActionResult<IEnumerable<ChatMessage>>> GetLastXPrivateMessages(string chatChannelIdString, int num)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid currentUserId))
                return Unauthorized(new { message = "Usuário não autenticado." });

            if (!Guid.TryParse(chatChannelIdString, out Guid chatChannelId))
                return BadRequest(new { message = "ID do canal de chat privado inválido." });

            if (num <= 0)
                return BadRequest(new { message = "Número de mensagens deve ser positivo." });

            var channel = await chatService.GetChatChannelByIdAsync(chatChannelId);
            if (channel == null || !channel.IsPrivate || (channel.User1Id != currentUserId && channel.User2Id != currentUserId))
            {
                return Forbid("Você não tem permissão para acessar mensagens deste canal privado.");
            }

            var messages = await chatService.GetLastXMessagesAsync(chatChannelId, num);
            return Ok(messages);
        }
    }

    // DTO para o request de iniciar chat privado
    public class StartPrivateChatRequest
    {
        [Required]
        public string RecipientUserId { get; set; } = string.Empty;
    }
}
