using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SafeVault.Data;
using SafeVault.Models.Scaffolded;
using SafeVault.Utilities;

namespace SafeVault.Pages;

[Authorize(Roles = "Admin")]
public class FindUsersModel(SafeVaultContext db) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? SearchUsername { get; set; }

    public IReadOnlyList<User> SearchResults { get; private set; } = [];

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
}