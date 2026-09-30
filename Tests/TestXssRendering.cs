using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Tests;

[TestFixture]
public class TestXssRendering
{
    [Test]
    public async Task RegistrationEncodesScriptLikeUsernameInRenderedForm()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        var getResponse = await client.GetAsync("/Register");
        var getHtml = await getResponse.Content.ReadAsStringAsync();

        var tokenMatch = Regex.Match(
            getHtml,
            """<input\b(?=[^>]*name="__RequestVerificationToken")(?=[^>]*value="([^"]+)")[^>]*>""");
        Assert.That(tokenMatch.Success, Is.True);

        var token = WebUtility.HtmlDecode(tokenMatch.Groups[1].Value);
        using var form = new FormUrlEncodedContent(
        [
            new("Input.Username", "<script>alert(1)</script>"),
            new("Input.Email", "test@example.com"),
            new("Input.Password", "correct horse battery staple"),
            new("Input.PasswordConfirmation", "correct horse battery staple"),
            new("__RequestVerificationToken", token)
        ]);

        var postResponse = await client.PostAsync("/Register", form);
        var renderedHtml = await postResponse.Content.ReadAsStringAsync();

        Assert.That(postResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(renderedHtml, Does.Contain("&lt;script&gt;alert(1)&lt;/script&gt;"));
        Assert.That(renderedHtml, Does.Not.Contain("<script>alert(1)</script>"));
    }
}