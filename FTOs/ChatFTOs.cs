using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;

namespace perenne.FTOs
{

    public record ChatMessageFTO(
        string Message,
        bool IsRead,
        bool IsDelivered,
        Guid ChatChannelId
        );
}
