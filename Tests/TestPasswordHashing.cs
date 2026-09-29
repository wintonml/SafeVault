using SafeVault.Services.Implementations;

namespace Tests;

[TestFixture]
public class TestPasswordHashing
{
    private readonly PasswordHashingService passwordHashingService = new();

    [Test]
    public void HashesAndVerifiesPassword()
    {
        const string password = "correct horse battery staple";

        var passwordHash = passwordHashingService.Hash(password);

        Assert.That(passwordHash, Is.Not.EqualTo(password));
        Assert.That(passwordHashingService.Verify(password, passwordHash), Is.True);
        Assert.That(passwordHashingService.Verify("wrong password", passwordHash), Is.False);
    }
}