using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SafeVault.Data;
using SafeVault.Models.ViewModels;
using SafeVault.Services.Interfaces;

namespace SafeVault.Pages;

public class LoginModel(
    SafeVaultContext db,
    IPasswordHashingService passwordHashingService) : PageModel
{
    [BindProperty]
    public UserLogin Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated != true)
            return Page();

        return Url.IsLocalUrl(ReturnUrl)
            ? LocalRedirect(ReturnUrl!)
            : RedirectToPage("/Index");
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        Input.UsernameOrEmail = Input.UsernameOrEmail?.Trim();
        if (!TryValidateModel(Input, nameof(Input)))
            return Page();

        var user = await db.Users
            .AsNoTracking()
            .Where(user => user.Username == Input.UsernameOrEmail || user.Email == Input.UsernameOrEmail)
            .OrderByDescending(user => user.Username == Input.UsernameOrEmail)
            .FirstOrDefaultAsync(cancellationToken);

        if (user?.Password is null || !passwordHashingService.Verify(Input.Password!, user.Password))
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return Page();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.Username ?? user.Email ?? string.Empty),
            new(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(claims, "SafeVault.Cookie");
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(
            "SafeVault.Cookie",
            principal,
            new AuthenticationProperties { IsPersistent = Input.RememberMe });

        return Url.IsLocalUrl(ReturnUrl)
            ? LocalRedirect(ReturnUrl!)
            : RedirectToPage("/Index");
    }
}
