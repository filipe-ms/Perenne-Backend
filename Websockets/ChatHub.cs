using Microsoft.AspNetCore.SignalR;

namespace perenne.Websockets
{
    /* MODIFICAR */
    // ADICIONAR [Authorize]
    public class ChatHub : Hub
    {
        public const string ChatHubPath = "/chatHub";

        public async Task JoinChannel(string channelId)
        {
            /* MODIFICAR */
            // VERIFICAR JWT E SE O USUÁRIO EXISTE NO BANCO / É VÁLIDO E MEMBRO DO GRUPO

            // ESSE GRUPO É DO SIGNALR, NÃO TEM NADA A VER COM NOSSOS GRUPOS
            await Groups.AddToGroupAsync(Context.ConnectionId, channelId);
        }

        public async Task LeaveChannel(string channelId)
        {
            // ESSE GRUPO É DO SIGNALR, NÃO TEM NADA A VER COM NOSSOS GRUPOS
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, channelId);
        }

        // TROCAR USER POR JWT AQUI EMBAIXO
        public async Task SendMessage(string channelId, string user, string message) 
        {
            /* MODIFICAR */
            // VERIFICAR JWT E SE O USUÁRIO EXISTE NO
            // BANCO / É VÁLIDO E MEMBRO DO GRUPO
            // E SE TEM PERMISSÃO DE POSTAR

            // ESSE GRUPO É DO SIGNALR, NÃO TEM NADA A VER COM NOSSOS GRUPOS
            // Envia a mensagem apenas para o grupo do canal
            await Clients.Group(channelId).SendAsync("ReceiveMessage", user, message);
        }
    }
}
