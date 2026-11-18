using System.ComponentModel.DataAnnotations;

namespace SmartStock.Models;

public class User
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Full name is required")]
    [StringLength(80)]
    public string FullName { get; set; } = "";

    [Required(ErrorMessage = "Email address is required")]
    [EmailAddress(ErrorMessage = "Enter a valid email address")]
    [StringLength(120)]
    public string Email { get; set; } = "";

    [Required]
    public string PasswordHash { get; set; } = "";

    [StringLength(40)]
    public string Role { get; set; } = "Manager";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
