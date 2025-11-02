namespace UserManagement.Application.DTOs;

public record UpdateUserDto(
    string? Email,
    string? FirstName,
    string? LastName,
    bool? IsActive
);
