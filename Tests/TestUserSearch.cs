using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SafeVault.Data;
using SafeVault.Models.Scaffolded;
using SafeVault.Pages;

namespace Tests;

[TestFixture]
public class TestUserSearch
{
    private SqliteConnection _connection = null!;
    private SafeVaultContext _db = null!;
    private IndexModel _page = null!;

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
        _db.Users.Add(new User { Username = "admin", Email = "admin@example.com" });
        await _db.SaveChangesAsync();
        _page = new IndexModel(_db);
    }

    [TearDown]
    public async Task TearDown()
    {
        await _db.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Test]
    public async Task SearchTreatsInjectionTextAsLiteral()
    {
        _page.SearchUsername = "admin' OR 1=1--";

        await _page.OnGetSearchAsync(CancellationToken.None);

        Assert.That(_page.SearchResults, Is.Empty);
    }

    [Test]
    public async Task SearchFindsExactUsername()
    {
        _page.SearchUsername = "admin";

        await _page.OnGetSearchAsync(CancellationToken.None);

        Assert.That(_page.SearchResults, Has.Count.EqualTo(1));
        Assert.That(_page.SearchResults[0].Email, Is.EqualTo("admin@example.com"));
    }

    [Test]
    public async Task DefaultGetHandlerSearchesWhenUsernameQueryIsPresent()
    {
        _page.SearchUsername = "admin";

        await _page.OnGetAsync(CancellationToken.None);

        Assert.That(_page.SearchResults, Has.Count.EqualTo(1));
        Assert.That(_page.SearchResults[0].Email, Is.EqualTo("admin@example.com"));
    }
}