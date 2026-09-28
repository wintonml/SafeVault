using System.ComponentModel.DataAnnotations;
using NUnit.Framework;
using SafeVault.Utilities;
using SafeVault.Models.ViewModels;

namespace Tests;

[TestFixture]
public class TestInputValidation
{
    [Test]
    public void TestForSQLInjection()
    {
        var input = new UserInput
        {
            Username = "admin' OR 1=1--",
            Email = "user@example.com"
        };

        Sanitize(input);

        Assert.That(IsValid(input), Is.False);
    }

    [Test]
    public void TestForXSS()
    {
        var input = new UserInput
        {
            Username = "<script>alert('xss')</script>",
            Email = "user@example.com"
        };

        Sanitize(input);

        Assert.That(IsValid(input), Is.False);
    }

    [Test]
    public void SanitizesControlCharactersAndTrimsValues()
    {
        var input = new UserInput
        {
            Username = "  Alice\u0000  ",
            Email = "  alice@example.com\t"
        };

        Sanitize(input);

        Assert.That(input.Username, Is.EqualTo("Alice"));
        Assert.That(input.Email, Is.EqualTo("alice@example.com"));
        Assert.That(IsValid(input), Is.True);
    }

    [Test]
    public void AcceptsCapitalizedUsername()
    {
        var input = new UserInput
        {
            Username = "Admin",
            Email = "admin@example.com"
        };

        Assert.That(IsValid(input), Is.True);
    }

    private static void Sanitize(UserInput input)
    {
        input.Username = InputSanitizer.Sanitize(input.Username);
        input.Email = InputSanitizer.Sanitize(input.Email);
    }

    private static bool IsValid(UserInput input)
    {
        var validationResults = new List<ValidationResult>();
        return Validator.TryValidateObject(
            input,
            new ValidationContext(input),
            validationResults,
            validateAllProperties: true);
    }
}