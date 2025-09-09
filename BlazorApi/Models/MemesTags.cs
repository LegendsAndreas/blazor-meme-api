using System.ComponentModel.DataAnnotations;

namespace BlazorApi.Models;

public class MemesTags
{
    [Key]
    public int Id { get; set; }
    
    public int MemeId { get; set; }
    public Meme Meme { get; set; } = null!;
    
    public int TagId { get; set; }
    public Tag Tag { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}