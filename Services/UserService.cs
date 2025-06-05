using perenne.DTOs;
using perenne.Interfaces;
using perenne.Models;
using perenne.Repositories;

namespace perenne.Services;

public class UserService(IUserRepository userRepository) : IUserService
{
    public async Task<User> GetUserByIdAsync(Guid id)
    {
        var user = await userRepository.GetUserByIdAsync(id);
        return user ?? throw new Exception("User not found");
    }

    public async Task<IEnumerable<Group>> GetGroupsByUserIdAsync(Guid userId)
    {
        return await userRepository.GetGroupsByUserIdAsync(userId);
    }

    public Guid ParseUserId(string? str)
    {
        if (string.IsNullOrEmpty(str))
            throw new ArgumentNullException($"[UserService] O parâmetro 'str' está nulo ou vazio. Um identificador de usuário é obrigatório.");
        if (!Guid.TryParse(str, out var guid))
            throw new ArgumentException($"[UserService] O valor fornecido não é um GUID válido.");
        return guid;
    }

    public async Task<UserInfoDto?> GetUserInfoAsync(Guid userId)
    {
        var userEntity = await userRepository.GetUserByIdAsync(userId);

        if (userEntity == null)
        {
            return null;
        }

        var groups = await userRepository.GetGroupsByUserIdAsync(userId);

        var userInfoDto = new UserInfoDto
        {
            Email = userEntity.Email,
            IsValidated = userEntity.IsValidated,
            IsBanned = userEntity.IsBanned,
            FirstName = userEntity.FirstName,
            LastName = userEntity.LastName,
            CPF = userEntity.CPF,
            ProfilePictureUrl = userEntity.ProfilePictureUrl,
            Groups = groups?.Select(g => g.Name).ToList() ?? new List<string>(), // pega os nomes dos grupos
            CreatedAt = userEntity.CreatedAt
        };

        return userInfoDto;
    }

    // Recuperação de senha
    public async Task<(bool Success, string? Token, string? ErrorMessage)> InitiatePasswordResetAsync(string email)
    {
        var user = await userRepository.GetUserByEmailAsync(email);
        if (user == null)
        {
            return (false, null, "Usuário não encontrado com este e-mail.");
        }

        user.PasswordResetToken = Guid.NewGuid().ToString("N"); // "N" para formato sem hífens
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1); // Token expira em 1 hora

        var updateSuccess = await userRepository.UpdateUserAsync(user);
        if (!updateSuccess)
        {
            return (false, null, "Erro ao salvar o token de redefinição.");
        }
        
        // Idealmente, o envio de e-mail seria aqui.
        // Por agora, retornamos o token para que o controller possa (conceitualmente) usá-lo.
        // Em um cenário real, você injetaria um IEmailService e chamaria algo como:
        // await _emailService.SendPasswordResetEmailAsync(user.Email, user.PasswordResetToken);
        // E não retornaria o token diretamente na resposta da API por segurança, apenas uma mensagem de sucesso.

        return (true, user.PasswordResetToken, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> ResetPasswordAsync(string token, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return (false, "Token inválido ou ausente.");
        }

        var user = await userRepository.GetUserByPasswordResetTokenAsync(token);

        if (user == null)
        {
            return (false, "Token de redefinição inválido.");
        }

        if (user.PasswordResetTokenExpiry < DateTime.UtcNow)
        {
            // Limpa o token expirado para segurança
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;
            await userRepository.UpdateUserAsync(user);
            return (false, "Token de redefinição expirado.");
        }

        // Atualiza a senha (sem hash, conforme solicitado)
        user.Password = newPassword;
        user.PasswordResetToken = null; // Invalida o token após o uso
        user.PasswordResetTokenExpiry = null;

        var updateSuccess = await userRepository.UpdateUserAsync(user);
        if (!updateSuccess)
        {
            return (false, "Erro ao atualizar a senha.");
        }

        return (true, null);
    }

}
