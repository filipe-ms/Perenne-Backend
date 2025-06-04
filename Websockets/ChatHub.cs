using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using perenne.Interfaces;
using perenne.Models;
using perenne.Services;

namespace perenne.Websockets
{
    [Authorize]
    public class ChatHub(
        IUserService userService, 
        IGroupService groupService, 
        IMessageCacheService messageCacheService,
        IChatService chatService) : Hub
    {
        public const string ChatHubPath = "/chatHub";

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (string.IsNullOrEmpty(userId))
            {
                Context.Abort();
                return;
            }
            await base.OnConnectedAsync();
        }

        public async Task JoinChannel(string channelIdString)
        {
            var userIdentifier = Context.UserIdentifier;
            if (string.IsNullOrEmpty(userIdentifier))
            {
                throw new HubException("[JoinChannel] User not authenticated or identifier missing.");
            }

            if (!Guid.TryParse(userIdentifier, out var userIdGuid))
            {
                throw new HubException("[JoinChannel] Invalid user identifier format.");
            }

            if (!Guid.TryParse(channelIdString, out var channelId))
            {
                throw new HubException("[JoinChannel] Invalid channel identifier. Must be a GUID for the group.");
            }

            try
            {
                var group = await groupService.GetGroupByIdAsync(channelId) ?? throw new HubException($"[ChatHub] Channel / Group with ID <{channelIdString}> not found.");
                var isMember = group.Members != null && group.Members.Any(m => m.UserId == userIdGuid);
                if (!isMember) throw new HubException("[ChatHub] You are not a member of this channel.");

                // "Groups" is from SignalR, not related to our application's Groups
                await Groups.AddToGroupAsync(Context.ConnectionId, channelIdString);

            }
            catch (KeyNotFoundException ex)
            {
                throw new HubException($"[ChatHub] Channel / Group with ID <{channelIdString}> not found: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new HubException($"[ChatHub] Error trying to join channel ID <{channelIdString}>: {ex.Message}");
            }
        }

        public async Task LeaveChannel(string channelId)
        {
            // "Groups" is from SignalR, not related to our application's Groups
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, channelId);
        }

        public async Task SendMessage(string channelIdString, string messageContent)
        {
            var userIdentifier = Context.UserIdentifier;

            if (string.IsNullOrEmpty(userIdentifier))
                throw new HubException("User not authenticated or identifier missing.");

            if (!Guid.TryParse(userIdentifier, out var userIdGuid))
                throw new HubException("Invalid user identifier format.");

            if (string.IsNullOrWhiteSpace(messageContent))
                throw new HubException("Message cannot be empty.");

            if (!Guid.TryParse(channelIdString, out var groupIdGuid)) // Assuming channelIdString is the GroupId
                throw new HubException("Invalid channel identifier format. Expected a GUID for the group.");

            try
            {
                var group = await groupService.GetGroupByIdAsync(groupIdGuid) ?? throw new HubException($"Channel (Group) with ID '{channelIdString}' not found.");
                if (group.ChatChannel == null)
                    throw new HubException($"Chat channel not configured for group '{channelIdString}'.");

                var member = (group.Members?.FirstOrDefault(m => m.UserId == userIdGuid)) ?? throw new HubException("You are not a member of this channel.");
                if (member.IsBlocked || member.IsMuted)
                    throw new HubException("You are not allowed to send messages in this channel (blocked or muted).");

                var sender = await userService.GetUserByIdAsync(userIdGuid) ?? throw new HubException("Sender user details not found, although authenticated.");
                var senderDisplayName = $"{sender.FirstName} {sender.LastName}";

                var chatMessage = new ChatMessage
                {
                    Message = messageContent,
                    ChatChannelId = group.ChatChannel.Id,
                    ChatChannel = group.ChatChannel,

                    CreatedAt = DateTime.UtcNow,
                    CreatedById = userIdGuid,

                    IsDelivered = true,
                    IsRead = false,
                    IsDeleted = false,
                };

                await messageCacheService.HandleChatMessageReceived(chatMessage);

                await Clients.Group(channelIdString).SendAsync("ReceiveMessage", senderDisplayName, messageContent, chatMessage.CreatedAt, userIdGuid.ToString());
            }
            catch (Exception ex)
            {
                throw new HubException($"An error occurred while sending your message in channel <{channelIdString}>. Please try again later. Details: {ex.Message}");
            }
        }

        // Mensagens privadas
        public async Task SendPrivateMessage(string recipientUserIdString, string messageContent)
        {
            var senderUserIdentifier = Context.UserIdentifier;

            if (string.IsNullOrEmpty(senderUserIdentifier) || !Guid.TryParse(senderUserIdentifier, out var senderUserIdGuid))
                throw new HubException("Remetente não autenticado ou identificador inválido.");

            if (string.IsNullOrWhiteSpace(messageContent))
                throw new HubException("A mensagem não pode estar vazia.");

            if (!Guid.TryParse(recipientUserIdString, out var recipientUserIdGuid))
                throw new HubException("Identificador do destinatário inválido.");

            if (senderUserIdGuid == recipientUserIdGuid)
                throw new HubException("Não é possível enviar uma mensagem privada para si mesmo através deste método.");

            try
            {
                var privateChatChannel = await chatService.GetOrCreatePrivateChatChannelAsync(senderUserIdGuid, recipientUserIdGuid);
                var sender = await userService.GetUserByIdAsync(senderUserIdGuid)
                             ?? throw new HubException("Detalhes do remetente não encontrados.");
                var senderDisplayName = $"{sender.FirstName} {sender.LastName}";
                var chatMessage = new ChatMessage
                {
                    Message = messageContent,
                    ChatChannelId = privateChatChannel.Id,
                    ChatChannel = privateChatChannel,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = senderUserIdGuid,
                    IsDelivered = true,
                    IsRead = false,
                    IsDeleted = false,
                };

                var savedMessage = await messageCacheService.HandleChatMessageReceived(chatMessage);
                var payload = new
                {
                    chatChannelId = privateChatChannel.Id.ToString(),
                    senderId = senderUserIdGuid.ToString(),
                    senderDisplayName,
                    message = savedMessage.Message,
                    createdAt = savedMessage.CreatedAt,
                    messageId = savedMessage.Id.ToString()
                };

                await Clients.User(recipientUserIdString).SendAsync("ReceivePrivateMessage", payload);
                await Clients.User(senderUserIdentifier).SendAsync("ReceivePrivateMessage", payload);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro em SendPrivateMessage: {ex.Message} {ex.StackTrace}");
                await Clients.Caller.SendAsync("ReceiveMessageError", $"Erro ao enviar mensagem privada: {ex.Message}");
            }
        }
    }
}
