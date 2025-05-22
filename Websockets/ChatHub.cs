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
            // Add logic here if anything needs to be done when a user connects.
            var userId = Context.UserIdentifier;
            if (string.IsNullOrEmpty(userId)) // This is just for safety
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
                throw new HubException("[JoinChannel] User not authenticated or identifier missing.");
            }

            if (!Guid.TryParse(userIdentifier, out var userIdGuid))
            {
                throw new HubException("[JoinChannel] Invalid user identifier format.");
            }

            if (!Guid.TryParse(channelId, out var groupIdGuid)) // Assuming channelId is the GroupId
            {
                throw new HubException("[JoinChannel] Invalid channel identifier. Must be a GUID for the group.");
            }

            try
            {
                var group = await _groupService.GetGroupByIdAsync(groupIdGuid);
                if (group == null)
                {
                    throw new HubException($"[ChatHub] Channel / Group with ID <{channelId}> not found.");
                }

                var isMember = group.Members != null && group.Members.Any(m => m.UserId == userIdGuid);
                if (!isMember)
                {
                    throw new HubException("[ChatHub] You are not a member of this channel.");
                }

                // "Groups" is from SignalR, not related to our application's Groups
                await Groups.AddToGroupAsync(Context.ConnectionId, channelId);

            }
            catch (KeyNotFoundException ex)
            {
                throw new HubException($"[ChatHub] Channel / Group with ID <{channelId}> not found: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new HubException($"[ChatHub] Error trying to join channel ID <{channelId}>: {ex.Message}");
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
                var group = await _groupService.GetGroupByIdAsync(groupIdGuid);

                if (group == null)
                    throw new HubException($"Channel (Group) with ID '{channelIdString}' not found.");

                if (group.ChatChannel == null)
                    throw new HubException($"Chat channel not configured for group '{channelIdString}'.");

                var member = group.Members?.FirstOrDefault(m => m.UserId == userIdGuid);

                if (member == null)
                    throw new HubException("You are not a member of this channel.");

                if (member.IsBlocked || member.IsMutedInGroupChat)
                    throw new HubException("You are not allowed to send messages in this channel (blocked or muted).");

                var sender = await _userService.GetUserByIdAsync(userIdGuid);
                if (sender == null)
                    throw new HubException("Sender user details not found, although authenticated.");

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

                await _chatService.AddChatMessageAsync(chatMessage);

                await Clients.Group(channelIdString).SendAsync("ReceiveMessage", senderDisplayName, messageContent, chatMessage.CreatedAt, userIdGuid.ToString());
            }
            catch (Exception ex)
            {
                throw new HubException($"An error occurred while sending your message in channel <{channelIdString}>. Please try again later. Details: {ex.Message}");
            }
        }
    }
}
