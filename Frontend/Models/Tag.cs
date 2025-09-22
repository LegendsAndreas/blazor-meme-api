using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Frontend.Models;

[Table("Tags")]
public class Tag
{
    [Key] public int Id { get; set; }
    [StringLength(255)] public string Name { get; set; } = string.Empty;
    [StringLength(255)] public string AddedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<MemesTags> MemesTags { get; set; } = new List<MemesTags>();
}

public class TagsStatsDto
{
    public int TagCount { get; set; }
    public Dictionary<string, int> ContributedUsers { get; set; } = new();
    public Dictionary<string, int> Tags { get; set; } = new();
}