using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using IndustrialDeviceSystem.Core.Interfaces;

namespace IndustrialDeviceSystem.Infrastructure.Security;

public sealed class Pbkdf2PasswordHasher :
    IPasswordHasher
{
    private const string Algorithm =
        "PBKDF2-SHA256-V1";

    private const int IterationCount = 600_000;

    private const int MinimumAcceptedIterations =
        100_000;

    private const int MaximumAcceptedIterations =
        2_000_000;

    private const int SaltSize = 16;

    private const int HashSize = 32;

    public string HashPassword(string password)
    {
        ValidatePassword(password);

        byte[] salt =
            RandomNumberGenerator.GetBytes(
                SaltSize);

        byte[] passwordBytes =
            Encoding.UTF8.GetBytes(password);

        byte[] hash = Array.Empty<byte>();

        try
        {
            hash = Rfc2898DeriveBytes.Pbkdf2(
                passwordBytes,
                salt,
                IterationCount,
                HashAlgorithmName.SHA256,
                HashSize);

            return string.Join(
                "$",
                Algorithm,
                IterationCount.ToString(
                    CultureInfo.InvariantCulture),
                Convert.ToBase64String(salt),
                Convert.ToBase64String(hash));
        }
        finally
        {
            CryptographicOperations.ZeroMemory(
                passwordBytes);

            CryptographicOperations.ZeroMemory(
                salt);

            if (hash.Length > 0)
            {
                CryptographicOperations.ZeroMemory(
                    hash);
            }
        }
    }

    public bool VerifyPassword(
        string password,
        string storedPasswordHash)
    {
        if (string.IsNullOrEmpty(password) ||
            string.IsNullOrWhiteSpace(
                storedPasswordHash))
        {
            return false;
        }

        string[] parts =
            storedPasswordHash.Split('$');

        if (parts.Length != 4 ||
            !string.Equals(
                parts[0],
                Algorithm,
                StringComparison.Ordinal))
        {
            return false;
        }

        if (!int.TryParse(
                parts[1],
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out int iterations))
        {
            return false;
        }

        if (iterations <
                MinimumAcceptedIterations ||
            iterations >
                MaximumAcceptedIterations)
        {
            return false;
        }

        byte[] salt;
        byte[] expectedHash;

        try
        {
            salt = Convert.FromBase64String(
                parts[2]);

            expectedHash =
                Convert.FromBase64String(
                    parts[3]);
        }
        catch (FormatException)
        {
            return false;
        }

        if (salt.Length != SaltSize ||
            expectedHash.Length != HashSize)
        {
            CryptographicOperations.ZeroMemory(
                salt);

            CryptographicOperations.ZeroMemory(
                expectedHash);

            return false;
        }

        byte[] passwordBytes =
            Encoding.UTF8.GetBytes(password);

        byte[] actualHash =
            Array.Empty<byte>();

        try
        {
            actualHash =
                Rfc2898DeriveBytes.Pbkdf2(
                    passwordBytes,
                    salt,
                    iterations,
                    HashAlgorithmName.SHA256,
                    HashSize);

            return CryptographicOperations
                .FixedTimeEquals(
                    expectedHash,
                    actualHash);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(
                passwordBytes);

            CryptographicOperations.ZeroMemory(
                salt);

            CryptographicOperations.ZeroMemory(
                expectedHash);

            if (actualHash.Length > 0)
            {
                CryptographicOperations.ZeroMemory(
                    actualHash);
            }
        }
    }

    private static void ValidatePassword(
        string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException(
                "密码不能为空。",
                nameof(password));
        }

        if (password.Length < 8)
        {
            throw new ArgumentException(
                "密码长度不能少于8个字符。",
                nameof(password));
        }

        if (password.Length > 128)
        {
            throw new ArgumentException(
                "密码长度不能超过128个字符。",
                nameof(password));
        }
    }
}
