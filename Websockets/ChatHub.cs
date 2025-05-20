using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using perenne.Interfaces;
using perenne.Models;

namespace perenne.Websockets
{
    [Authorize]
    public class ChatHub : Hub
    {
        public const string ChatHubPath = "/chatHub";

        private readonly IChatService _chatService;
        private readonly IGroupService _groupService;
        private readonly IUserService _userService;

        public ChatHub(IUserService userService, IGroupService groupService, IChatService chatService)
        {
            _chatService = chatService;
            _groupService = groupService;
            _userService = userService;
        }

        public override async Task OnConnectedAsync()
        {
            // Adicionar lógica aqui se algo tiver que ser feito quando um usuário se conectar.
            var userId = Context.UserIdentifier;
            if (string.IsNullOrEmpty(userId)) // Isso está aqui só por segurança
            {
                Context.Abort();
                return;
            }
            await base.OnConnectedAsync();
        }

        public async Task JoinChannel(string channelId)
        {
            var userIdentifier = Context.UserIdentifier;
            if (string.IsNullOrEmpty(userIdentifier))
            {
                throw new HubException("[ChatHub] Usuário não autenticado ou sem identificador.");
            }

            if (!Guid.TryParse(userIdentifier, out var userIdGuid))
            {
                throw new HubException("[ChatHub] Formato de identificação inválido.");
            }

            if (!Guid.TryParse(channelId, out var groupIdGuid))
            {
                throw new HubException("[ChatHub] Identificador do canal inválido. Deve ser um GUID.");
            }

            try
            {
                var group = await _groupService.GetGroupByIdAsync(groupIdGuid);
                if (group == null)
                {
                    throw new HubException($"[ChatHub] Canal / Grupo com ID <{channelId}> não encontrado.");
                }

                var isMember = group.Members != null && group.Members.Any(m => m.UserId == userIdGuid);
                if (!isMember)
                {
                    throw new HubException("[ChatHub] Você não é membro deste canal.");
                }

                // "GROUPS É DO SIGNALR, NÃO TEM NADA A VER COM NOSSOS GRUPOS
                await Groups.AddToGroupAsync(Context.ConnectionId, channelId);

            }
            catch (KeyNotFoundException ex)
            {
                throw new HubException($"[ChatHub] Canal / Grupo com ID <{channelId}> não encontrado: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new HubException($"[ChatHub] Erro tentando entrar no canal ID <{channelId}>: {ex.Message}");
            }
        }

        public async Task LeaveChannel(string channelId)
        {
            // "GROUPS" É DO SIGNALR, NÃO TEM NADA A VER COM NOSSOS GRUPOS
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, channelId);
        }

        public async Task SendMessage(string channelIdString, string messageContent)
        {
            var userIdentifier = Context.UserIdentifier;

            if (string.IsNullOrEmpty(userIdentifier))
            {
                throw new HubException("User not authenticated or identifier missing.");
            }

            if (!Guid.TryParse(userIdentifier, out var userIdGuid))
            {
                throw new HubException("Invalid user identifier format.");
            }

            if (string.IsNullOrWhiteSpace(messageContent))
            {
                throw new HubException("Message cannot be empty.");
            }

            if (!Guid.TryParse(channelIdString, out var groupIdGuid))
            {
                throw new HubException("Invalid channel identifier format. Expected a GUID for the group.");
            }

            try
            {
                var group = await _groupService.GetGroupByIdAsync(groupIdGuid);
                if (group == null)
                {
                    throw new HubException($"Channel (Group) with ID '{channelIdString}' not found.");
                }

                // Os chats são criados assim que o grupo é criado
                // Idealmente isso não acontece, então tá aqui "só em caso de"
                if (group.ChatChannel == null)
                {
                    throw new HubException($"Chat channel not configured for group '{channelIdString}'.");
                }

                var member = group.Members?.FirstOrDefault(m => m.UserId == userIdGuid);
                if (member == null)
                {
                    throw new HubException("Você não é um membro deste canal.");
                }

                if (member.IsBlocked || member.IsMutedInGroupChat)
                {
                    throw new HubException("You are not allowed to send messages in this channel (blocked or muted).");
                }

                var sender = await _userService.GetUserByIdAsync(userIdGuid);
                if (sender == null)
                {
                    throw new HubException("Sender user details not found.");
                }

                var senderDisplayName = $"{sender.FirstName} {sender.LastName}";

                var chatMessage = new ChatMessage
                {
                    Message = messageContent,
                    ChatChannelId = group.ChatChannel.Id,
                    CreatedById = userIdGuid,
                    CreatedAt = DateTime.UtcNow,
                    IsDelivered = true,
                    IsRead = false,
                    ChatChannel = group.ChatChannel
                };

                await _chatService.AddChatMessageAsync(chatMessage);

                // O ID do grupo é utilizado como nome do grupo do SignalR.
                // SE LIGAR QUE GROUPS É DO SIGNALR, NÃO TEM NADA A VER COM NOSSOS GRUPOS
                await Clients.Group(channelIdString).SendAsync("ReceiveMessage", senderDisplayName, messageContent, chatMessage.CreatedAt, userIdGuid.ToString());
            }
            catch (KeyNotFoundException ex)
            {
                throw new HubException($"Erro processando mensagem do canal <{channelIdString}>: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new HubException($"Erro enviando sua mensagem no canal <{channelIdString}>: {ex.Message}");
            }
        }
    }
}
