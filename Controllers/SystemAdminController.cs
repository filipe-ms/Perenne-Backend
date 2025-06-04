using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using perenne.DTOs;
using perenne.Interfaces;
using perenne.Models;
using perenne.Utils;
using System.Security.Claims;

namespace perenne.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SystemAdminController(IGroupService groupService, IUserService userService) : ControllerBase
    {
        // [host]/api/systemadmin/creategroup
        [HttpPost(nameof(CreateGroup))]
        public async Task<ActionResult<GroupCreateDto>> CreateGroup([FromBody] GroupCreateDto dto)
        {
            var user = await GetCurrentUser();
            if (!IsSystemAdmin(user.SystemRole)) return Forbid("Apenas administradores podem criar grupos.");

            var createdGroup = await groupService.CreateGroupAsync(dto);

            return Ok(createdGroup);
        }

        // [host]/api/systemadmin/deletegroup
        [HttpDelete(nameof(DeleteGroup))]
        public async Task<ActionResult<bool>> DeleteGroup([FromBody] GroupDeleteDto dto)
        {
            var user = await GetCurrentUser();
            if (!IsSystemAdmin(user.SystemRole)) return Forbid("Apenas administradores podem remover grupos.");

            var groupId = groupService.ParseGroupId(dto.GroupIdString);

            return Ok(await groupService.DeleteGroupAsync(groupId));
        }

        // [host]/api/systemadmin/updategroup
        [HttpPut(nameof(UpdateGroup))]
        public async Task<ActionResult<GroupUpdateDto>> UpdateGroup([FromBody] GroupUpdateDto dto)
        {
            var requester = await GetCurrentUser();
            var groupId = groupService.ParseGroupId(dto.GroupIdString);
            if (!IsSystemAdmin(requester.SystemRole) && !await IsCurrentUserCoordinator(groupId)) return Forbid("Apenas administradores do sistema ou coordenadores do grupo podem atualizar grupos.");

            var group = await groupService.GetGroupByIdAsync(groupId);

            string newName = "";

            if (string.IsNullOrEmpty(dto.NewNameString)) newName = group.Name;
            if (dto.NewNameString!.Length != 0 && dto.NewNameString.Length < 4) return BadRequest("O nome do grupo deve ter ao menos 4 caracteres.");
            if (dto.NewNameString!.Length > 500) return BadRequest("O nome fornecido é muito grande. Reduza-o e tente novamente.");

            if (string.IsNullOrEmpty(newName)) newName = dto.NewNameString;

            group.Name = newName;
            group.Description = dto.NewDescriptionString;

            var result = await groupService.UpdateGroupAsync(group);
            return Ok(result);
        }

        // [host]/api/systemadmin/updateusersystemrole
        [HttpPatch(nameof(UpdateUserSystemRole))]
        public async Task<ActionResult<bool>> UpdateUserSystemRole([FromBody] SystemRoleDTO userRoleDto)
        {
            var requester = await GetCurrentUser();

            if (!IsSystemAdmin(requester.SystemRole)) return Forbid("Apenas administradores podem alterar cargos no sistema.");

            var targetId = userService.ParseUserId(userRoleDto.UserIdString);
            var targetUser = await userService.GetUserByIdAsync(targetId);

            SystemRole newRole = RoleUtils.NameToEnum<SystemRole>(userRoleDto.NewRoleString);

            if (!SystemRoleChangeValidation(requester.SystemRole, targetUser.SystemRole, newRole))
            {
                return BadRequest("Você não tem permissão para alterar o cargo do usuário especificado. Verifique os cargos envolvidos.");
            }

            if (newRole == SystemRole.None) return BadRequest("Cargo (Role) de sistema fornecido é inválido.");

            var result = await userService.UpdateUserRoleInSystemAsync(targetId, newRole);
            return Ok(result);
        }

        // [host]/api/systemadmin/updategroupmemberrole
        [HttpPatch(nameof(UpdateGroupMemberRole))]
        public async Task<ActionResult<bool>> UpdateGroupMemberRole([FromBody] MemberRoleDTO memberDto)
        {
            var requester = await GetCurrentUser();
            var groupId = groupService.ParseGroupId(memberDto.GroupIdString);

            // Só passa se for System Admin ou Coordenador de grupo
            if (!IsSystemAdmin(requester.SystemRole) && !await IsCurrentUserCoordinator(groupId)) return Forbid("Apenas administradores do sistema ou coordenadores do grupo podem atualizar cargos de membros de grupos.");

            GroupRole newRole = RoleUtils.NameToEnum<GroupRole>(memberDto.NewRoleString);
            if(newRole == GroupRole.None) return BadRequest("Cargo (Role) fornecido é inválido.");

            var targetId = userService.ParseUserId(memberDto.UserIdString);
            
            if (targetId == requester.Id) return BadRequest("Um usuário não pode alterar seu próprio cargo.");

            var result = await groupService.UpdateGroupMemberRoleAsync(targetId, groupId, newRole);

            return Ok(result);
        }

        // [host]/api/systemadmin/approvegroupjoinrequest
        [HttpPost(nameof(ApproveGroupJoinRequest))]
        public async Task<ActionResult> ApproveGroupJoinRequest([FromBody] string requestIdString)
        {
            if (!Guid.TryParse(requestIdString, out var requestId))
                return BadRequest("Invalid Request GUID");

            var joinRequest = await groupService.GetJoinRequestByIdAsync(requestId);

            if (joinRequest == null) return NotFound("Join request not found.");

            var user = await GetCurrentUser();
            if (!IsSystemAdmin(user.SystemRole) && !await IsCurrentUserCoordinator(joinRequest.GroupId))
                return Forbid("Apenas administradores do sistema ou coordenadores do grupo podem aprovar solicitações de adesão.");

            var newMember = await groupService.ApproveJoinRequestAsync(requestId, user.Id);
            
            if (newMember == null) return StatusCode(500, "Failed to approve request and add member.");
            
            return Ok(new { Message = "Solicitação aprovada. Usuário adicionado ao grupo.", MemberId = newMember.UserId, newMember.GroupId });
        }

        // [host]/api/systemadmin/rejectgroupjoinrequest
        [HttpPost(nameof(RejectJoinRequest))]
        public async Task<ActionResult> RejectJoinRequest([FromBody] string requestIdString)
        {
            if (!Guid.TryParse(requestIdString, out var requestId))
                return BadRequest("Invalid Request GUID");

            var joinRequest = await groupService.GetJoinRequestByIdAsync(requestId);
            if (joinRequest == null) return NotFound("Join request not found.");

            var user = await GetCurrentUser();
            if (!IsSystemAdmin(user.SystemRole) && !await IsCurrentUserCoordinator(joinRequest.GroupId))
                return Forbid("Apenas administradores do sistema ou coordenadores do grupo podem rejeitar solicitações de adesão.");

            await groupService.RejectJoinRequestAsync(requestId, user.Id);
            return Ok(new { Message = "Solicitação rejeitada." });
        }

        // [host]/api/systemadmin/{groupIdString}/getpendingmembers
        [HttpGet("{groupIdString}/getpendingmembers")]
        public async Task<ActionResult<IEnumerable<object>>> GetPendingMembersForGroup(string groupIdString)
        {
            var user = await GetCurrentUser();
            var groupId = groupService.ParseGroupId(groupIdString);
            if (!IsSystemAdmin(user.SystemRole) && !await IsCurrentUserCoordinator(groupId))
                return Forbid("Apenas administradores do sistema ou coordenadores do grupo podem ver membros pendentes de grupos.");

            var requests = await groupService.GetPendingRequestsForGroupAsync(groupId, user.Id);

            var response = requests.Select(r => new
            {
                RequestId = r.Id,
                r.UserId,
                UserName = $"{r.User?.FirstName} {r.User?.LastName}",
                UserEmail = r.User?.Email,
                r.RequestedAt,
                r.Message
            });

            return Ok(response);
        }

        // [host]/api/systemadmin/muteuseringroupchat
        [HttpPatch(nameof(MuteUserInGroupChat))]
        public async Task<ActionResult> MuteUserInGroupChat([FromBody] MuteChatMemberDTO dto)
        {   
            var groupId = groupService.ParseGroupId(dto.GroupIdString);
            var group = await groupService.GetGroupByIdAsync(groupId);
            
            var target = userService.ParseUserId(dto.MemberIdString);
            
            if (!await HasAuthorityInGroup(group.Id) || !await CanBeMuted(target, group.Id))
                return Forbid("Apenas administradores do sistema, moderadores ou coordenadores do grupo podem silenciar usuários em grupos.");

            if (dto.Minutes < 1) return BadRequest("O período de mute deve ser maior do que um minuto.");

            var groupMemberToMute = await groupService.GetGroupMemberAsync(target, groupId);

            groupMemberToMute.MutedUntil = DateTime.UtcNow.AddMinutes(dto.Minutes);
            groupMemberToMute.MutedBy = GetCurrentUserId();

            var result = await groupService.MuteUserInGroupAsync(groupMemberToMute);
            return Ok(result);
        }

        // [host]/api/systemadmin/muteuseringroupchat
        [HttpPatch(nameof(MuteUserInGroupChat))]
        public async Task<ActionResult> UnmuteUserInGroupChat([FromBody] UnmuteChatMemberDTO dto)
        {
            var groupId = groupService.ParseGroupId(dto.GroupIdString);
            var group = await groupService.GetGroupByIdAsync(groupId);

            var target = userService.ParseUserId(dto.MemberIdString);

            if (!await HasAuthorityInGroup(group.Id) || !await CanBeMuted(target, group.Id))
                return Forbid("Apenas administradores do sistema, moderadores ou coordenadores do grupo podem remover mute de usuários em grupos.");

            var groupMemberToMute = await groupService.GetGroupMemberAsync(target, groupId);

            groupMemberToMute.MutedUntil = null;
            groupMemberToMute.MutedBy = Guid.Empty;

            var result = await groupService.UnmuteUserInGroupAsync(groupMemberToMute);
            return Ok(result);
        }



        // Utils
        private Guid GetCurrentUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userIdString)) throw new ArgumentNullException(nameof(userIdString), "[SystemAdminController] O parâmetro 'userIdString' está nulo ou vazio. Um identificador de usuário é obrigatório.");
            return userService.ParseUserId(userIdString);
        }
        private async Task<User> GetCurrentUser()
        {
            var userId = GetCurrentUserId();
            return await userService.GetUserByIdAsync(userId);
        }
        private async Task<bool> IsCurrentUserCoordinator(Guid groupId)
        {
            var userId = GetCurrentUserId();
            var membership = await groupService.GetGroupMemberAsync(userId, groupId);
            return membership.Role == GroupRole.Coordinator;
        }
        private static bool IsSystemAdmin(SystemRole role)
        {
            return role == SystemRole.SuperAdmin || role == SystemRole.Admin;
        }
        private static bool SystemRoleChangeValidation(SystemRole requesterRole, SystemRole targetRole, SystemRole newTargetRole)
        {
            switch (requesterRole)
            {
                // Cargo de confiança.
                // Pode modificar qualquer cargo, inclusive criar outros SuperAdmins.
                case SystemRole.SuperAdmin:
                    return true;

                // Admins podem modificar qualquer cargo abaixo de Admins.
                case SystemRole.Admin:
                    if (targetRole != SystemRole.SuperAdmin && targetRole != SystemRole.Admin) return true;
                    else return false;

                default:
                    return false;
            }
        }

        private async Task<bool> HasAuthorityInGroup(Guid groupId)
        {
            var user = await GetCurrentUser();
            if (user.SystemRole == SystemRole.SuperAdmin ||
                user.SystemRole == SystemRole.Admin ||
                user.SystemRole == SystemRole.Moderator)
                return true;

            var groupMember = await groupService.GetGroupMemberAsync(user.Id, groupId);
            if (groupMember.Role == GroupRole.Coordinator)
                return true;

            return false;
        }

        private async Task<bool> CanBeMuted(Guid targetId, Guid groupId)
        {
            var target = await userService.GetUserByIdAsync(targetId);
            if (target.SystemRole == SystemRole.SuperAdmin ||
                target.SystemRole == SystemRole.Admin)
                return false;

            var groupMember = await groupService.GetGroupMemberAsync(targetId, groupId);
            if (groupMember.Role == GroupRole.Coordinator)
                return false;

            return true;
        }
    }
}
