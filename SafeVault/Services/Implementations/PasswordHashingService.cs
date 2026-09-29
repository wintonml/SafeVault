using BC = BCrypt.Net.BCrypt;
using SafeVault.Services.Interfaces;

namespace SafeVault.Services.Implementations;

public sealed class PasswordHashingService : IPasswordHashingService
{
    public string Hash(string password)
    {
        return BC.EnhancedHashPassword(password);
    }

    public bool Verify(string password, string passwordHash)
    {
        return BC.EnhancedVerify(password, passwordHash);
    }
}