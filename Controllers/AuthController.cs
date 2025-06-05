using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using perenne.DTOs;
using perenne.Interfaces; // Peixoto
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace perenne.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IOptions<JwtSettings> jwtSettings, IUserService userService /*, IEmailService emailService */) : ControllerBase // Adicionado IUserService
    {
        private readonly JwtSettings _jwtSettings = jwtSettings.Value;
        // private readonly IEmailService _emailService = emailService; // Para envio de email

        [HttpPost("generate-token")] // Nome da rota mais explícito
        public IActionResult GenerateToken([FromBody] TokenGenerationRequest request)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, request.UserId.ToString()), // Supondo que UserId é um Guid
                new Claim(JwtRegisteredClaimNames.Email, request.Email),
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(_jwtSettings.ExpirationHours),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            return Ok(new { Token = tokenString }); // Retornar um objeto é uma prática comum
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, token, errorMessage) = await userService.InitiatePasswordResetAsync(request.Email);

            if (!success)
            {
                // Mesmo se o usuário não for encontrado, é uma boa prática de segurança retornar uma mensagem genérica
                // para evitar a enumeração de e-mails. No entanto, para depuração/desenvolvimento,
                // você pode querer retornar o erro específico.
                // Para produção: return Ok(new { Message = "Se o e-mail existir em nosso sistema, um link de redefinição será enviado." });
                return BadRequest(new { Message = errorMessage ?? "Ocorreu um erro ao processar sua solicitação." });
            }

            // ---- INÍCIO: LÓGICA DE ENVIO DE E-MAIL (Conceitual) ----
            // Aqui você construiria o link e enviaria o e-mail.
            // O token é retornado pelo InitiatePasswordResetAsync APENAS PARA ESTE EXEMPLO.
            // EM PRODUÇÃO, o token NÃO deve ser retornado na resposta da API.
            // O serviço de usuário (ou um serviço de e-mail dedicado) lidaria com o envio.

            var resetLink = Url.Action("ResetPasswordPage", "Auth", new { token = token }, Request.Scheme);
            // Nota: "ResetPasswordPage" seria um endpoint GET no seu frontend ou um endpoint que serve a página de redefinição.
            // Para uma API, o link seria para o seu frontend, ex: https://seusite.com/reset-password?token={token}

            // Simulação do envio de e-mail:
            // await _emailService.SendPasswordResetEmailAsync(request.Email, token, resetLink);
            // Console.WriteLine($"Link de redefinição para {request.Email}: {resetLink}"); // Apenas para debug

            // Em vez de retornar o token, você retornaria uma mensagem de sucesso.
            // O token foi retornado aqui para ilustrar, mas é uma falha de segurança em um sistema real.
            // Remova o token da resposta em produção.
            return Ok(new { Message = "Solicitação de redefinição de senha recebida. Se o e-mail estiver registrado, você receberá um link para redefinir sua senha.", DevelopmentOnly_ResetToken = token, DevelopmentOnly_ResetLink = resetLink });
            // ---- FIM: LÓGICA DE ENVIO DE E-MAIL ----
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, errorMessage) = await userService.ResetPasswordAsync(request.Token, request.NewPassword);

            if (!success)
            {
                return BadRequest(new { Message = errorMessage ?? "Não foi possível redefinir a senha." });
            }

            return Ok(new { Message = "Senha redefinida com sucesso." });
        }
    }
}