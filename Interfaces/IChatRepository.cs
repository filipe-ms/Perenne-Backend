namespace perenne.Interfaces
{
    public interface IChatRepository
    {
        Task<ChatChannel> AddChatChannelAsync(ChatChannel chat);
    }
}
