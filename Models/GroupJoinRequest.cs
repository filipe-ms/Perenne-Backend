using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace perenne.Models
{
    public class GroupJoinRequest
    {
        [Required, Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [Required]
        public Guid UserId { get; set; }
        public virtual User User { get; set; }

        [Required]
        public Guid GroupId { get; set; } // Id do grupo ao qual o usuário está solicitando adesão
        public virtual Group Group { get; set; } // Grupo ao qual o usuário está solicitando adesão

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        public Guid? HandledByUserId { get; set; } // Id de quem aprovou ou recusou
        public virtual User? HandledByUser { get; set; } // Usuário que aprovou ou recusou
        public DateTime? HandledAt { get; set; } // Data em que o pedido foi aprovado ou recusado

        [MaxLength(500)]
        public string? Message { get; set; }
    }
}