# User Management API

A modern .NET 8 API application with user authentication, CRUD operations, and comprehensive testing.

## Architecture

The project follows Clean Architecture principles with the following structure:

- **UserManagement.Core**: Domain entities and interfaces
- **UserManagement.Application**: Business logic and DTOs
- **UserManagement.Infrastructure**: Data access and external services
- **UserManagement.API**: Web API controllers and configuration
- **UserManagement.CLI**: Command-line tool for user management
- **UserManagement.Tests.Unit**: Unit tests for services and infrastructure
- **UserManagement.Tests.Integration**: Integration tests for API endpoints

## Features

- JWT-based authentication
- User CRUD operations (Create, Read, Update, Delete)
- Secure password hashing (PBKDF2 with SHA512)
- SQLite database with Entity Framework Core
- Comprehensive unit and integration tests
- Swagger/OpenAPI documentation
- CLI tool for creating users directly in the database

## Prerequisites

- .NET 8.0 SDK
- Any IDE (Visual Studio, VS Code, Rider)

## Getting Started

### 1. Build the Solution

```bash
dotnet build
```

### 2. Run the API

```bash
cd src/UserManagement.API
dotnet run
```

The API will be available at:
- HTTPS: https://localhost:7xxx
- HTTP: http://localhost:5xxx
- Swagger UI: https://localhost:7xxx/swagger

### 3. Create a User via CLI

You can create users directly using the CLI tool:

```bash
cd src/UserManagement.CLI
dotnet run
```

Follow the interactive prompts to create a user.

Alternatively, use the provided script:

```bash
./create-user.sh
```

### 4. Test the API

#### Create a User (via API)

```bash
curl -X POST https://localhost:7xxx/api/users \
  -H "Content-Type: application/json" \
  -d '{
    "username": "johndoe",
    "email": "john@example.com",
    "password": "Password123!",
    "firstName": "John",
    "lastName": "Doe"
  }'
```

#### Login

```bash
curl -X POST https://localhost:7xxx/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "johndoe",
    "password": "Password123!"
  }'
```

This will return a JWT token. Use this token in subsequent requests.

#### Get All Users (Requires Authentication)

```bash
curl -X GET https://localhost:7xxx/api/users \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

#### Get User by ID (Requires Authentication)

```bash
curl -X GET https://localhost:7xxx/api/users/{userId} \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

#### Update User (Requires Authentication)

```bash
curl -X PUT https://localhost:7xxx/api/users/{userId} \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "newemail@example.com",
    "firstName": "Jane",
    "lastName": "Smith",
    "isActive": true
  }'
```

#### Delete User (Requires Authentication)

```bash
curl -X DELETE https://localhost:7xxx/api/users/{userId} \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

## Running Tests

### Run All Tests

```bash
dotnet test
```

### Run Unit Tests Only

```bash
dotnet test tests/UserManagement.Tests.Unit
```

### Run Integration Tests Only

```bash
dotnet test tests/UserManagement.Tests.Integration
```

### Run Tests with Code Coverage

```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Configuration

### API Configuration (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=usermanagement.db"
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong!",
    "Issuer": "UserManagementAPI",
    "Audience": "UserManagementAPI",
    "ExpiryMinutes": "60"
  }
}
```

**Important**: Change the `SecretKey` in production to a secure, randomly generated value.

## Project Structure

```
.
├── src/
│   ├── UserManagement.API/          # Web API
│   │   ├── Controllers/             # API Controllers
│   │   ├── Program.cs               # Application entry point
│   │   └── appsettings.json         # Configuration
│   ├── UserManagement.Application/  # Business logic
│   │   ├── DTOs/                    # Data Transfer Objects
│   │   └── Services/                # Application services
│   ├── UserManagement.Core/         # Domain layer
│   │   ├── Entities/                # Domain entities
│   │   └── Interfaces/              # Repository & service interfaces
│   ├── UserManagement.Infrastructure/ # Data access & infrastructure
│   │   ├── Data/                    # DbContext
│   │   ├── Repositories/            # Repository implementations
│   │   └── Security/                # Security services
│   └── UserManagement.CLI/          # Command-line tool
├── tests/
│   ├── UserManagement.Tests.Unit/   # Unit tests
│   └── UserManagement.Tests.Integration/ # Integration tests
└── UserManagement.sln               # Solution file
```

## Security Features

- **Password Hashing**: PBKDF2 with SHA512 and 100,000 iterations
- **JWT Authentication**: Secure token-based authentication
- **Input Validation**: Comprehensive validation on all endpoints
- **Unique Constraints**: Username and email uniqueness enforced

## API Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | /api/users | Create a new user | No |
| POST | /api/auth/login | Login and get JWT token | No |
| GET | /api/users | Get all users | Yes |
| GET | /api/users/{id} | Get user by ID | Yes |
| PUT | /api/users/{id} | Update user | Yes |
| DELETE | /api/users/{id} | Delete user | Yes |

## Testing Strategy

### Unit Tests
- Service layer logic
- Password hashing
- Business rules validation

### Integration Tests
- End-to-end API testing
- Authentication flows
- CRUD operations
- Error handling

## Development

### Adding a New Feature

1. Define interfaces in `UserManagement.Core`
2. Implement services in `UserManagement.Application`
3. Add infrastructure in `UserManagement.Infrastructure`
4. Create controllers in `UserManagement.API`
5. Write unit tests in `UserManagement.Tests.Unit`
6. Write integration tests in `UserManagement.Tests.Integration`

## License

MIT
