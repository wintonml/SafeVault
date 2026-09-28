using System.ComponentModel.DataAnnotations;

namespace SafeVault.Models.ViewModels;

public class UserInput
{
    [Required]
    [StringLength(100)]
    [RegularExpression(@"^[A-Za-z0-9 _.'-]+$", ErrorMessage = "Use English letters, numbers, spaces, periods, apostrophes, underscores, or hyphens.")]
    public string? Username { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }
}