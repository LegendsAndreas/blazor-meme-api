using System.ComponentModel.DataAnnotations;

namespace BlazorApi.Models;

public class Tag
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<MemesTags> MemesTags { get; set; } = new List<MemesTags>();
}