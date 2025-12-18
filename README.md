# User Management API

A production-ready REST API for user authentication and management built with .NET 8 and Clean Architecture.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![License](https://img.shields.io/badge/License-GPL--2.0-blue)
![Architecture](https://img.shields.io/badge/Architecture-Clean-blue)

## Features

- **JWT Authentication** - Secure token-based auth with configurable expiration
- **User CRUD** - Complete user management operations
- **Password Security** - PBKDF2 with SHA512 (100,000 iterations)
- **OpenAPI/Swagger** - Interactive API documentation
- **SQLite Database** - Zero-configuration persistent storage
- **CLI Tool** - Create users directly from command line

## Requirements

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Quick Start

```bash
# Clone and run
git clone <repository-url>
cd csharp-dotnet-learning
dotnet run --project src/UserManagement.API
```

API available at `http://<your-ip>:7000` | Swagger UI at `/swagger`

## Installation

### Development Setup

```bash
# 1. Clone the repository
git clone <repository-url>
cd csharp-dotnet-learning

# 2. Restore dependencies
dotnet restore

# 3. Build the solution
dotnet build

# 4. Run tests
dotnet test

# 5. Start the API (with hot reload)
dotnet watch run --project src/UserManagement.API
```

### Production Deployment

```bash
# 1. Publish release build
dotnet publish src/UserManagement.API -c Release -o ./publish

# 2. Configure environment variables
export ASPNETCORE_ENVIRONMENT=Production
export ConnectionStrings__DefaultConnection="Data Source=/path/to/usermanagement.db"
export JwtSettings__SecretKey="<your-secure-secret-key-min-32-chars>"

# 3. Run the application
cd publish
dotnet UserManagement.API.dll
```

#### Production Configuration

Create `appsettings.Production.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=/var/data/usermanagement.db"
  },
  "JwtSettings": {
    "SecretKey": "<generate-secure-random-key>",
    "Issuer": "UserManagementAPI",
    "Audience": "UserManagementAPI",
    "ExpiryMinutes": "60"
  }
}
```

> **Security**: Always use a unique, randomly generated secret key in production (minimum 32 characters).

## API Reference

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `POST` | `/api/users` | Create user | No |
| `POST` | `/api/auth/login` | Login (get JWT) | No |
| `GET` | `/api/users` | List all users | Yes |
| `GET` | `/api/users/{id}` | Get user by ID | Yes |
| `PUT` | `/api/users/{id}` | Update user | Yes |
| `DELETE` | `/api/users/{id}` | Delete user | Yes |

### Example: Create User & Login

```bash
# Create user
curl -X POST http://localhost:7000/api/users \
  -H "Content-Type: application/json" \
  -d '{"username":"john","email":"john@example.com","password":"Password123!","firstName":"John","lastName":"Doe"}'

# Login
curl -X POST http://localhost:7000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"john","password":"Password123!"}'

# Use token for authenticated requests
curl -X GET http://localhost:7000/api/users \
  -H "Authorization: Bearer <token>"
```

### CLI User Creation

```bash
# Interactive user creation
dotnet run --project src/UserManagement.CLI

# Or use the helper script
./create-user.sh
```

## Architecture

```
src/
├── UserManagement.Core           # Domain entities & interfaces
├── UserManagement.Application    # Business logic & DTOs
├── UserManagement.Infrastructure # EF Core, repositories, security
├── UserManagement.API            # Controllers & configuration
└── UserManagement.CLI            # Command-line tool

tests/
├── UserManagement.Tests.Unit        # xUnit + Moq + FluentAssertions
└── UserManagement.Tests.Integration # WebApplicationFactory tests
```

**Dependency flow**: Core ← Application ← Infrastructure ← API

## Testing

```bash
# All tests
dotnet test

# Unit tests only
dotnet test tests/UserManagement.Tests.Unit

# Integration tests only
dotnet test tests/UserManagement.Tests.Integration

# With coverage report
dotnet test --collect:"XPlat Code Coverage"
```

## License

This project is licensed under the [GNU General Public License v2.0](LICENSE).
