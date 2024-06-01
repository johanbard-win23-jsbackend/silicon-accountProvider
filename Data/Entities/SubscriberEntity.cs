using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

public class SubscriberEntity
{
    [Key]
    public int Id { get; set; }
    [Column(TypeName = "nvarchar(150)")]
    public string Email { get; set; } = null!;
    public bool isActive { get; set; } = true;
}
