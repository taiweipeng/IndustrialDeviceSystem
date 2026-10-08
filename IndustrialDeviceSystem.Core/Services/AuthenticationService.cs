using IndustrialDeviceSystem.Core.Interfaces;
using IndustrialDeviceSystem.Core.Models;

namespace IndustrialDeviceSystem.Core.Services;

public sealed class AuthenticationService
{
    private readonly IUserRepository
        _userRepository;

    private readonly IPasswordHasher
        _passwordHasher;

    public AuthenticationService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher)
    {
        ArgumentNullException.ThrowIfNull(
            userRepository);

        ArgumentNullException.ThrowIfNull(
            passwordHasher);

        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public UserSession? Authenticate(
        string userName,
        string password)
    {
        if (string.IsNullOrWhiteSpace(userName) ||
            string.IsNullOrEmpty(password))
        {
            return null;
        }

        UserAccount? user =
            _userRepository.GetByUserName(
                userName.Trim());

        if (user is null ||
            !user.IsEnabled)
        {
            return null;
        }

        bool passwordIsValid =
            _passwordHasher.VerifyPassword(
                password,
                user.PasswordHash);

        if (!passwordIsValid)
        {
            return null;
        }

        return new UserSession(
            user.UserId,
            user.UserName,
            user.Role);
    }
}
