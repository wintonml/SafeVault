using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SafeVault.Data;
using SafeVault.Utilities;
using SafeVault.Models.ViewModels;
using DatabaseUser = SafeVault.Models.Scaffolded.User;

namespace SafeVault.Pages;

public class IndexModel(SafeVaultContext db) : PageModel
{
    [BindProperty]
    public UserInput Input { get; set; } = new();

    public string? StatusMessage { get; private set; }

    [BindProperty(SupportsGet = true)]
    public string? SearchUsername { get; set; }

    public IReadOnlyList<DatabaseUser> SearchResults { get; private set; } = [];

    public bool SearchSubmitted { get; private set; }

    public string? SearchError { get; private set; }

    public Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        return SearchUsername is null
            ? Task.FromResult<IActionResult>(Page())
            : SearchUsersAsync(cancellationToken);
    }

    public Task<IActionResult> OnGetSearchAsync(CancellationToken cancellationToken)
    {
        return SearchUsersAsync(cancellationToken);
    }

    private async Task<IActionResult> SearchUsersAsync(CancellationToken cancellationToken)
    {
        SearchSubmitted = true;
        SearchUsername = InputSanitizer.Sanitize(SearchUsername);

        if (string.IsNullOrWhiteSpace(SearchUsername))
        {
            SearchError = "Enter a username to search.";
            return Page();
        }

        if (SearchUsername.Length > 100)
        {
            SearchError = "Username searches must be 100 characters or fewer.";
            return Page();
        }

        SearchResults = await db.Users
            .AsNoTracking()
            .Where(user => user.Username == SearchUsername)
            .OrderBy(user => user.UserId)
            .Take(20)
            .ToListAsync(cancellationToken);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Input.Username = InputSanitizer.Sanitize(Input.Username);
        Input.Email = InputSanitizer.Sanitize(Input.Email);

        ModelState.Clear();
        if (!TryValidateModel(Input, nameof(Input)))
            return Page();

        db.Users.Add(new DatabaseUser
        {
            Username = Input.Username!,
            Email = Input.Email!
        });

        await db.SaveChangesAsync();
        StatusMessage = "User saved successfully.";
        return Page();

    }
}
