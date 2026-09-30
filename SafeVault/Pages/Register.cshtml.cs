using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SafeVault.Data;
using SafeVault.Models.ViewModels;
using SafeVault.Services.Interfaces;
using SafeVault.Utilities;
using DatabaseUser = SafeVault.Models.Scaffolded.User;

namespace SafeVault.Pages;

public class RegisterModel(
    SafeVaultContext db,
    IPasswordHashingService passwordHashingService) : PageModel
{
    [BindProperty]
    public UserInput Input { get; set; } = new();

    public IActionResult OnGet()
    {
        return User.Identity?.IsAuthenticated == true
            ? RedirectToPage("/Index")
            : Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToPage("/Index");

        Input.Username = InputSanitizer.NormaliseIdentifier(Input.Username);
        Input.Email = InputSanitizer.NormaliseIdentifier(Input.Email);

        ModelState.Clear();
        if (!TryValidateModel(Input, nameof(Input)))
            return Page();

        db.Users.Add(new DatabaseUser
        {
            Username = Input.Username!,
            Email = Input.Email!,
            Password = passwordHashingService.Hash(Input.Password!)
        });

        await db.SaveChangesAsync();
        return RedirectToPage("/Login");
    }
}