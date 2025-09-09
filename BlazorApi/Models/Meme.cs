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
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}