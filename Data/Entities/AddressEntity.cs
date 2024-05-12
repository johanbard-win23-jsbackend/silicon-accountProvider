using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

public class AddressEntity
{
    [Key]
    public int Id { get; set; }

    [Column(TypeName = "nvarchar(150)")]
    public string? Address1 { get; set; }

    [Column(TypeName = "nvarchar(150)")]
    public string? Address2 { get; set; }

    [Column(TypeName = "varchar(5)")]
    public string? PostalCode { get; set; }

    [Column(TypeName = "nvarchar(100)")]
    public string? City { get; set; }

    public virtual UserEntity? User { get; set; }

}
