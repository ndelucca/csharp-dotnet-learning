using UserManagement.Application.DTOs;
using UserManagement.Core.Interfaces;

namespace UserManagement.Application.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<AuthResultDto?> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByUsernameAsync(loginDto.Username, cancellationToken);

        if (user == null || !user.IsActive)
        {
            return null;
        }

        if (!_passwordHasher.VerifyPassword(loginDto.Password, user.PasswordHash))
        {
            return null;
        }

        var token = _tokenService.GenerateToken(user);
        var userDto = new UserDto(
            user.Id,
            user.Username,
            user.Email,
            user.FirstName,
            user.LastName,
            user.CreatedAt,
            user.UpdatedAt,
            user.IsActive
        );

        return new AuthResultDto(token, userDto);
    }
}
