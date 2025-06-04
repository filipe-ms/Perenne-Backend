namespace perenne.FTOs
{

    public record ChatMessageFTO(
        string Message,
        bool IsRead,
        bool IsDelivered,
        Guid ChatChannelId
        );
}
