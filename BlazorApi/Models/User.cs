using System.ComponentModel.DataAnnotations;

namespace BlazorApi.Models;

public class User
{
    [Key]
    public string Id { get; set; }
    public required string Email { get; set; }
    public required string HashedPassword { get; set; }
    public string? Salt { get; set; }
    public DateTime LastLogin { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string RoleId { get; set; } = string.Empty;
    public virtual Role? Roles { get; set; }
    
    public class LogOnDto
    {
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [Required(ErrorMessage = "Email is required")]
        public string email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Password is required")]
        public string password { get; set; } = string.Empty;
    }
    
    public class RegisterDto
    {
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
        
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
    public class LoginResponseDto
    {
        public string message { get; set; }
        public string token { get; set; }
    }
}