using System.ComponentModel.DataAnnotations;

namespace SafeVault.Models.ViewModels;

public class UserLogin
{
    [Required]
    [StringLength(100)]
    [Display(Name = "Username or email")]
    public string? UsernameOrEmail { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string? Password { get; set; }

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }
}