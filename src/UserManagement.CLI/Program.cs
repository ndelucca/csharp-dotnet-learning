using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserManagement.Application.DTOs;
using UserManagement.Application.Services;
using UserManagement.Core.Interfaces;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories;
using UserManagement.Infrastructure.Security;

// Build configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

// Setup dependency injection
var services = new ServiceCollection();

// Configure Entity Framework
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

// Register services
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<IPasswordHasher, PasswordHasher>();
services.AddScoped<ITokenService, TokenService>();
services.AddScoped<UserService>();
services.AddSingleton<IConfiguration>(configuration);

var serviceProvider = services.BuildServiceProvider();

// Ensure database is created
using (var scope = serviceProvider.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated();
}

Console.WriteLine("=== User Management CLI - Create User ===\n");

// Get user input
Console.Write("Username: ");
var username = Console.ReadLine()?.Trim();
if (string.IsNullOrEmpty(username))
{
    Console.WriteLine("Error: Username is required.");
    return 1;
}

Console.Write("Email: ");
var email = Console.ReadLine()?.Trim();
if (string.IsNullOrEmpty(email))
{
    Console.WriteLine("Error: Email is required.");
    return 1;
}

Console.Write("Password: ");
var password = ReadPassword();
Console.WriteLine();
if (string.IsNullOrEmpty(password))
{
    Console.WriteLine("Error: Password is required.");
    return 1;
}

Console.Write("First Name: ");
var firstName = Console.ReadLine()?.Trim() ?? string.Empty;

Console.Write("Last Name: ");
var lastName = Console.ReadLine()?.Trim() ?? string.Empty;

// Create user
try
{
    using var scope = serviceProvider.CreateScope();
    var userService = scope.ServiceProvider.GetRequiredService<UserService>();

    var createUserDto = new CreateUserDto(
        username,
        email,
        password,
        firstName,
        lastName
    );

    var user = await userService.CreateAsync(createUserDto);

    Console.WriteLine("\n✓ User created successfully!");
    Console.WriteLine($"  ID: {user.Id}");
    Console.WriteLine($"  Username: {user.Username}");
    Console.WriteLine($"  Email: {user.Email}");
    Console.WriteLine($"  Name: {user.FirstName} {user.LastName}");
    Console.WriteLine($"  Created At: {user.CreatedAt}");

    return 0;
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"\n✗ Error: {ex.Message}");
    return 1;
}
catch (Exception ex)
{
    Console.WriteLine($"\n✗ Unexpected error: {ex.Message}");
    return 1;
}

static string ReadPassword()
{
    var password = string.Empty;
    ConsoleKey key;

    do
    {
        var keyInfo = Console.ReadKey(intercept: true);
        key = keyInfo.Key;

        if (key == ConsoleKey.Backspace && password.Length > 0)
        {
            password = password[0..^1];
            Console.Write("\b \b");
        }
        else if (!char.IsControl(keyInfo.KeyChar))
        {
            password += keyInfo.KeyChar;
            Console.Write("*");
        }
    } while (key != ConsoleKey.Enter);

    return password;
}
