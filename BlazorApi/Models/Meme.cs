using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BlazorApi.Models;

public class Meme
{
    [Key]
    public int Id { get; set; }
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;
    [StringLength(255)]
    public string Extension { get; set; } = string.Empty;
    [StringLength(255)]
    public string MimeType { get; set; } = string.Empty;
    public required byte[] FileData { get; set; }
    public ICollection<MemesTags> MemesTags { get; set; } = new List<MemesTags>();
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class UploadMemeDto
{
    [Required]
    public IFormFile File { get; set; }

    [Required]
    public string Name { get; set; }

    public List<string> Tags { get; set; } = new();
}

public class MemesStatsDto
{
    public int MemesCount { get; set; }
    public int JpgCount { get; set; }
    public int PngCount { get; set; }
    public int GifCount { get; set; }
    public int WebpCount { get; set; }
    public int VideosCount { get; set; }
}