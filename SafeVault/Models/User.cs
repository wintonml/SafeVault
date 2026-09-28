using System.ComponentModel.DataAnnotations;

namespace SafeVault.Models;

public class User
{
    public int UserId { get; set; }

    [Required, StringLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(100)]
    public string Email { get; set; } = string.Empty;
}