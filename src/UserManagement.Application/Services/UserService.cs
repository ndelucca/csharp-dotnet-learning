using UserManagement.Application.DTOs;
using UserManagement.Core.Entities;
using UserManagement.Core.Interfaces;

namespace UserManagement.Application.Services;

public class UserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        return user != null ? MapToDto(user) : null;
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        return users.Select(MapToDto);
    }

    public async Task<UserDto> CreateAsync(CreateUserDto createUserDto, CancellationToken cancellationToken = default)
    {
        // Check if username already exists
        var existingUser = await _userRepository.GetByUsernameAsync(createUserDto.Username, cancellationToken);
        if (existingUser != null)
        {
            throw new InvalidOperationException($"Username '{createUserDto.Username}' already exists.");
        }

        // Check if email already exists
        existingUser = await _userRepository.GetByEmailAsync(createUserDto.Email, cancellationToken);
        if (existingUser != null)
        {
            throw new InvalidOperationException($"Email '{createUserDto.Email}' already exists.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = createUserDto.Username,
            Email = createUserDto.Email,
            PasswordHash = _passwordHasher.HashPassword(createUserDto.Password),
            FirstName = createUserDto.FirstName,
            LastName = createUserDto.LastName,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var createdUser = await _userRepository.CreateAsync(user, cancellationToken);
        return MapToDto(createdUser);
    }

    public async Task<UserDto> UpdateAsync(Guid id, UpdateUserDto updateUserDto, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID '{id}' not found.");
        }

        if (updateUserDto.Email != null)
        {
            // Check if new email already exists for another user
            var existingUser = await _userRepository.GetByEmailAsync(updateUserDto.Email, cancellationToken);
            if (existingUser != null && existingUser.Id != id)
            {
                throw new InvalidOperationException($"Email '{updateUserDto.Email}' already exists.");
            }
            user.Email = updateUserDto.Email;
        }

        if (updateUserDto.FirstName != null) user.FirstName = updateUserDto.FirstName;
        if (updateUserDto.LastName != null) user.LastName = updateUserDto.LastName;
        if (updateUserDto.IsActive.HasValue) user.IsActive = updateUserDto.IsActive.Value;

        user.UpdatedAt = DateTime.UtcNow;

        var updatedUser = await _userRepository.UpdateAsync(user, cancellationToken);
        return MapToDto(updatedUser);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _userRepository.DeleteAsync(id, cancellationToken);
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto(
            user.Id,
            user.Username,
            user.Email,
            user.FirstName,
            user.LastName,
            user.CreatedAt,
            user.UpdatedAt,
            user.IsActive
        );
    }
}
