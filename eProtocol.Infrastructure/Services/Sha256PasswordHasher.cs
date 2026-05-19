using System.Security.Cryptography;
using System.Text;
using eProtocol.Application.Abstractions;

namespace eProtocol.Infrastructure.Services;

public sealed class Sha256PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }

    public bool Verify(string hashedPassword, string password)
    {
        var computed = Hash(password);
        return StringComparer.OrdinalIgnoreCase.Equals(hashedPassword, computed);
    }
}
