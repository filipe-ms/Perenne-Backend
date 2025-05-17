namespace perenne.Interfaces
{
    public interface IChatService
    {
        Task<ChatChannel> CreateChatChannelAsync(ChatChannel channel);
    }
}
