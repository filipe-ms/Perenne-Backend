using perenne.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public abstract class Entity
{
    [Required, Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public required DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Foreign key IDs
    public Guid? CreatedById { get; set; }
    public Guid? UpdatedById { get; set; }

    // Navigation properties
    public User? CreatedBy { get; set; }
    public User? UpdatedBy { get; set; }
}
