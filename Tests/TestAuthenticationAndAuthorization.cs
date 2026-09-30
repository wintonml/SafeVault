using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SafeVault.Data;
using SafeVault.Models.Scaffolded;
using SafeVault.Models.ViewModels;
using SafeVault.Pages;
using SafeVault.Services.Implementations;

namespace Tests;

[TestFixture]
public class TestAuthenticationAndAuthorization
{
    private const string ValidPassword = "correct horse battery staple";

    private SqliteConnection _connection = null!;
    private SafeVaultContext _db = null!;
    private ServiceProvider _services = null!;
    private PasswordHashingService _passwordHashingService = null!;

    [SetUp]
    public async Task SetUp()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        await _connection.OpenAsync();

        var options = new DbContextOptionsBuilder<SafeVaultContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new SafeVaultContext(options);
        await _db.Database.EnsureCreatedAsync();

        _passwordHashingService = new PasswordHashingService();
        _db.Users.Add(new User
        {
            Username = "regular-user",
            Email = "user@example.com",
            Password = _passwordHashingService.Hash(ValidPassword),
            Role = "User"
        });
        await _db.SaveChangesAsync();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorization();
        services.AddMvcCore().AddDataAnnotations();
        _services = services.BuildServiceProvider();
    }

    [TearDown]
    public async Task TearDown()
    {
        await _services.DisposeAsync();
        await _db.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Test]
    public async Task LoginWithUnknownUsernameIsRejected()
    {
        var page = CreateLoginPage();
        page.Input = new UserLogin
        {
            UsernameOrEmail = "missing-user",
            Password = ValidPassword
        };

        var result = await page.OnPostAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<PageResult>());
        Assert.That(page.ModelState.Values.SelectMany(value => value.Errors)
            .Any(error => error.ErrorMessage == "Invalid username or password."), Is.True);
    }

    [Test]
    public async Task LoginWithIncorrectPasswordIsRejected()
    {
        var page = CreateLoginPage();
        page.Input = new UserLogin
        {
            UsernameOrEmail = "regular-user",
            Password = "incorrect password"
        };

        var result = await page.OnPostAsync(CancellationToken.None);

        Assert.That(result, Is.TypeOf<PageResult>());
        Assert.That(page.ModelState.Values.SelectMany(value => value.Errors)
            .Any(error => error.ErrorMessage == "Invalid username or password."), Is.True);
    }

    [Test]
    public async Task AnonymousUserCannotAccessHomePage()
    {
        var authorized = await IsAuthorizedAsync(typeof(IndexModel), CreatePrincipal());

        Assert.That(authorized, Is.False);
    }

    [Test]
    public async Task RegularUserCanAccessHomePage()
    {
        var authorized = await IsAuthorizedAsync(typeof(IndexModel), CreatePrincipal("User"));

        Assert.That(authorized, Is.True);
    }

    [Test]
    public async Task RegularUserCannotAccessFindUsersPage()
    {
        var authorized = await IsAuthorizedAsync(typeof(FindUsersModel), CreatePrincipal("User"));

        Assert.That(authorized, Is.False);
    }

    [Test]
    public async Task AdminCanAccessFindUsersPage()
    {
        var authorized = await IsAuthorizedAsync(typeof(FindUsersModel), CreatePrincipal("Admin"));

        Assert.That(authorized, Is.True);
    }

    private LoginModel CreateLoginPage()
    {
        var httpContext = new DefaultHttpContext { RequestServices = _services };
        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new PageActionDescriptor());

        return new LoginModel(_db, _passwordHashingService)
        {
            PageContext = new PageContext(actionContext)
        };
    }

    private async Task<bool> IsAuthorizedAsync(Type pageType, ClaimsPrincipal user)
    {
        var authorizationData = pageType.GetCustomAttributes<AuthorizeAttribute>(inherit: true);
        var policyProvider = _services.GetRequiredService<IAuthorizationPolicyProvider>();
        var policy = await AuthorizationPolicy.CombineAsync(policyProvider, authorizationData);
        var authorizationService = _services.GetRequiredService<IAuthorizationService>();

        var result = await authorizationService.AuthorizeAsync(user, resource: null, policy!);
        return result.Succeeded;
    }

    private static ClaimsPrincipal CreatePrincipal(string? role = null)
    {
        if (role is null)
            return new ClaimsPrincipal(new ClaimsIdentity());

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "regular-user"),
            new Claim(ClaimTypes.Role, role)
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
    }
}