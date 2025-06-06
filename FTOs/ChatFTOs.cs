namespace perenne.FTOs
{
    public record ChatMessageFTO(
        string FirstName,
        string LastName,
        string Message,
        bool IsRead,
        bool IsDelivered,
        Guid ChatChannelId,
        DateTime CreatedAt,
        Guid CreatedById
        );
}
