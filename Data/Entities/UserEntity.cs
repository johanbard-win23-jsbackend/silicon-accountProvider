using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

public class UserEntity : IdentityUser
{
    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string ProfileImg { get; set; } = "avatar.png";

    [Required]
    [Column(TypeName = "nvarchar(50)")]
    public string FirstName { get; set; } = null!;

    [Required]
    [Column(TypeName = "nvarchar(50)")]
    public string LastName { get; set; } = null!;

    [Column(TypeName = "nvarchar(20)")]
    public string? Phone { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string? Bio { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime RegistrationDate { get; set; }

    [ForeignKey(nameof(Address))]
    public int? AddressId { get; set; }

    public virtual AddressEntity? Address { get; set; }
}
