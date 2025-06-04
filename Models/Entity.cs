using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace perenne.Models
{
    public abstract class Entity
    {
        [Required, Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Foreign key IDs
        public Guid? CreatedById { get; set; }
        public Guid? UpdatedById { get; set; }

        // Navigation properties
        public User? CreatedBy { get; set; }
        public User? UpdatedBy { get; set; }
    }
}