using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using perenne.Interfaces;
using perenne.Models;
using System.Security.Claims;
using perenne.DTOs;
using perenne.FTOs;

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
        public async Task<ActionResult<IEnumerable<ChatMessageFTO>>> GetCachedMessages(string groupIdString)
        {
            if (!Guid.TryParse(groupIdString, out Guid groupId))
                return BadRequest(new { message = "Id de Grupo inválido na URL." });

            var group = await groupService.GetGroupByIdAsync(groupId);
            if (group == null || group.ChatChannel == null)
                return NotFound(new { message = "Grupo ou canal de chat do grupo não encontrado." });

            var chatId = group.ChatChannel.Id;
            var messages = await messageCacheService.GetMessagesByChatChannelIdAsync(chatId);

            var response = new List<ChatMessageFTO>();

            foreach (var msg in messages)
            {
                var createdById = msg.CreatedById ?? Guid.Empty;
                var user = await userService.GetUserByIdAsync(createdById);

                response.Add(new ChatMessageFTO(
                    FirstName: user?.FirstName ?? "Desconhecido",
                    LastName: user?.LastName ?? "",
                    Message: msg.Message,
                    IsRead: msg.IsRead,
                    IsDelivered: msg.IsDelivered,
                    ChatChannelId: msg.ChatChannelId,

                    CreatedAt: msg.CreatedAt,
                    CreatedById: createdById
                ));
            }

            return Ok(response);
        }

        // Pega as últimas X mensagens
        [HttpGet("{groupIdString}/getmessages/{num}")]
        public async Task<ActionResult<IEnumerable<ChatMessageFTO>>> GetLastXMessages(string groupIdString, int num)
        {
            if (!Guid.TryParse(groupIdString, out Guid groupId))
                return BadRequest(new { message = "Id de Grupo inválido na URL." });

            if (num <= 0)
                return BadRequest(new { message = "Número de mensagens deve ser positivo." });

            var group = await groupService.GetGroupByIdAsync(groupId);
            if (group == null || group.ChatChannel == null)
                return NotFound(new { message = "Grupo ou canal de chat do grupo não encontrado." });

            var chatId = group.ChatChannel.Id;
            var messages = await chatService.GetLastXMessagesAsync(chatId, num);

            var response = new List<ChatMessageFTO>();

            foreach (var msg in messages)
            {
                var createdById = msg.CreatedById ?? Guid.Empty;
                var user = await userService.GetUserByIdAsync(createdById);

                response.Add(new ChatMessageFTO(
                    FirstName: user?.FirstName ?? "Desconhecido",
                    LastName: user?.LastName ?? "",
                    Message: msg.Message,
                    IsRead: msg.IsRead,
                    IsDelivered: msg.IsDelivered,
                    ChatChannelId: msg.ChatChannelId,

                    CreatedAt: msg.CreatedAt,
                    CreatedById: createdById
                ));
            }

            return Ok(response);
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


            var chatChannel = await chatService.GetOrCreatePrivateChatChannelAsync(senderUserId, recipientUserId);
            var recipientUser = await userService.GetUserByIdAsync(recipientUserId);

            return Ok(new
            {
                chatChannel.Id,
                chatChannel.IsPrivate,
                chatChannel.User1Id,
                chatChannel.User2Id,
                OtherParticipant = new
                {
                    Id = recipientUser?.Id.ToString(),
                    DisplayName = recipientUser != null ? $"{recipientUser.FirstName} {recipientUser.LastName}" : "Usuário Desconhecido"
                }
            });

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
                        cc.Messages.First().CreatedAt,
                        SenderId = cc.Messages.First().CreatedById
                    } : null
                };
            });

            return Ok(await Task.WhenAll(result));
        }

        [HttpGet("private/{chatChannelIdString}/messages/{num}")]
        public async Task<ActionResult<IEnumerable<ChatMessageFTO>>> GetLastXPrivateMessages(string chatChannelIdString, int num)
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

            var response = new List<ChatMessageFTO>();

            foreach (var msg in messages)
            {
                var createdById = msg.CreatedById ?? Guid.Empty;
                var user = await userService.GetUserByIdAsync(createdById);

                response.Add(new ChatMessageFTO(
                    FirstName: user?.FirstName ?? "Desconhecido",
                    LastName: user?.LastName ?? "",
                    Message: msg.Message,
                    IsRead: msg.IsRead,
                    IsDelivered: msg.IsDelivered,
                    ChatChannelId: msg.ChatChannelId,

                    CreatedAt: msg.CreatedAt,
                    CreatedById: createdById
                ));
            }

            return Ok(response);
        }

        [HttpGet("private/{otherUserIdString}")]
        public async Task<ActionResult<IEnumerable<ChatMessageFTO>>> GetPrivateChatMessages(string otherUserIdString)
        {
            var currentUserId = GetCurrentUserId();

            if (!Guid.TryParse(otherUserIdString, out var otherUserId)) return BadRequest(new { message = "ID do outro usuário é inválido." });

            var channel = await chatService.GetPrivateChatChannelAsync(currentUserId, otherUserId);

            if (channel == null) return Ok(new List<ChatMessageFTO>());

            var messages = await messageCacheService.GetMessagesByChatChannelIdAsync(channel.Id);

            var user1 = await userService.GetUserByIdAsync(channel.User1Id ?? Guid.Empty);
            var user2 = await userService.GetUserByIdAsync(channel.User2Id ?? Guid.Empty);

            var response = messages.Select(msg =>
            {
                var sender = msg.CreatedById == user1?.Id ? user1 : user2;
                var senderId = msg.CreatedById ?? Guid.Empty;

                return new ChatMessageFTO(
                    FirstName: sender?.FirstName ?? "Desconhecido",
                    LastName: sender?.LastName ?? "",
                    Message: msg.Message,
                    IsRead: msg.IsRead,
                    IsDelivered: msg.IsDelivered,
                    ChatChannelId: msg.ChatChannelId,

                    CreatedAt: msg.CreatedAt,
                    CreatedById: senderId
                );
            });

            return Ok(response);
        }

        private Guid GetCurrentUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userIdString))
            {
                return Guid.Empty;
            }
            Guid.TryParse(userIdString, out Guid userId);
            return userId;
        }
    }
}