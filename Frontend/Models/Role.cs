using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Frontend.Models;

[Table("Roles")]
public class Role
{
    [Key]
    public string Id { get; set; }
    [StringLength(255)]
    public required string Name { get; set; } = string.Empty;
    [StringLength(255)]
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public virtual ICollection<User> Users { get; set; } = new List<User>();

    public static class Names
    {
        public const string User = "User";
        public const string CleaningStaff = "CleaningStaff";
        public const string Reception = "Reception";
        public const string Admin = "Admin";
    }
    
    public string ResolveRoleId(string roleId)
    {
        return roleId switch
        {
            "1" => Names.User,
            "2" => Names.CleaningStaff,
            "3" => Names.Reception,
            "4" => Names.Admin,
            _ => "Unknown"
        };
    }

}