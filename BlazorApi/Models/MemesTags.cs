using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorApi.Models;

[Table("MemesTags")]
public class MemesTags
{
    [Key]
    public int Id { get; set; }
    
    public int MemeId { get; set; }
    public Meme Meme { get; set; } = null!;
    
    public int TagId { get; set; }
    public Tag Tag { get; set; } = null!;
    [StringLength(255)]
    public string createdBy { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}