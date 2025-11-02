namespace UserManagement.Application.DTOs;

public record UserDto(
    Guid Id,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    bool IsActive
);
