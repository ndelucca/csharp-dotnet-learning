namespace UserManagement.Application.DTOs;

public record CreateUserDto(
    string Username,
    string Email,
    string Password,
    string FirstName,
    string LastName
);
