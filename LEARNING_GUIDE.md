# C# and .NET Learning Guide - Understanding This Project

A comprehensive guide for experienced developers new to C# and .NET, using this User Management API as a learning resource.

## Table of Contents

1. [C# Fundamentals](#c-fundamentals)
2. [.NET Core Concepts](#net-core-concepts)
3. [Project Structure Deep Dive](#project-structure-deep-dive)
4. [Step-by-Step Code Walkthrough](#step-by-step-code-walkthrough)
5. [Key Differences from Other Languages](#key-differences-from-other-languages)
6. [Common Patterns in This Project](#common-patterns-in-this-project)

---

## C# Fundamentals

### 1. Basic Syntax

C# is a strongly-typed, object-oriented language similar to Java but with modern features.

```csharp
// Variable declaration with type inference
var username = "john";  // Type inferred as string
string email = "john@example.com";  // Explicit type

// Null safety (C# 8.0+)
string? nullableString = null;  // ? indicates nullable reference type
string nonNullableString = "must have value";  // Cannot be null

// Properties (auto-implemented)
public class User
{
    public Guid Id { get; set; }  // Auto-property with getter and setter
    public string Username { get; init; }  // init = can only be set during initialization
}
```

**Key Concepts:**

- `var`: Type inference (compiler determines type)
- `?`: Nullable reference types (C# 8.0+)
- `{ get; set; }`: Auto-implemented properties (no need for backing fields)

### 2. Records (C# 9.0+)

Records are immutable reference types, perfect for DTOs.

```csharp
// See: src/UserManagement.Application/DTOs/UserDto.cs
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
```

**Why use records?**

- Immutable by default
- Value-based equality (two records are equal if their values match)
- Concise syntax for data objects
- Perfect for DTOs (Data Transfer Objects)

**Equivalent in other languages:**

```python
# Python dataclass
@dataclass(frozen=True)
class UserDto:
    id: str
    username: str
    # ... etc
```

```typescript
// TypeScript
interface UserDto {
    readonly id: string;
    readonly username: string;
    // ... etc
}
```

### 3. Async/Await

C# has first-class support for asynchronous programming.

```csharp
// See: src/UserManagement.Application/Services/UserService.cs
public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
{
    var user = await _userRepository.GetByIdAsync(id, cancellationToken);
    return user != null ? MapToDto(user) : null;
}
```

**Key Points:**

- `async`: Method is asynchronous
- `await`: Waits for async operation without blocking thread
- `Task<T>`: Represents an async operation that returns `T`
- `Task`: Async operation with no return value
- `CancellationToken`: Allows cancelling long-running operations

### 4. Null-Conditional Operators

C# has powerful null-checking syntax.

```csharp
// Null-conditional operator (?.)
var length = username?.Length;  // Returns null if username is null

// Null-coalescing operator (??)
var name = firstName ?? "Unknown";  // Use "Unknown" if firstName is null

// Null-coalescing assignment (??=)
_cache ??= new Dictionary<string, string>();  // Assign only if null
```

### 5. LINQ (Language Integrated Query)

LINQ allows querying collections with SQL-like syntax.

```csharp
// See: src/UserManagement.Application/Services/UserService.cs
var users = await _userRepository.GetAllAsync(cancellationToken);
return users.Select(MapToDto);  // Transform each user to DTO
```

**Common LINQ methods:**

```csharp
// Filtering
var activeUsers = users.Where(u => u.IsActive);

// Projection (mapping)
var usernames = users.Select(u => u.Username);

// Ordering
var sortedUsers = users.OrderBy(u => u.CreatedAt);

// Aggregation
var count = users.Count();
var hasUsers = users.Any();
```

---

## .NET Core Concepts

### 1. Dependency Injection (DI)

.NET has built-in DI container. Services are registered in `Program.cs`.

```csharp
// See: src/UserManagement.API/Program.cs

// Service Registration
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<UserService>();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
```

**Service Lifetimes:**

1. **Transient** (`AddTransient`): New instance every time

    ```csharp
    builder.Services.AddTransient<IEmailService, EmailService>();
    // Use for: Lightweight, stateless services
    ```

2. **Scoped** (`AddScoped`): One instance per request

    ```csharp
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    // Use for: DbContext, repositories, request-scoped services
    ```

3. **Singleton** (`AddSingleton`): One instance for application lifetime
    ```csharp
    builder.Services.AddSingleton<IConfiguration>(configuration);
    // Use for: Configuration, caches, thread-safe services
    ```

**Constructor Injection:**

```csharp
// See: src/UserManagement.API/Controllers/UsersController.cs
public class UsersController : ControllerBase
{
    private readonly UserService _userService;
    private readonly ILogger<UsersController> _logger;

    // Constructor - DI container automatically provides these
    public UsersController(UserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }
}
```

### 2. Configuration System

Configuration comes from multiple sources (appsettings.json, environment variables, etc.)

```csharp
// Reading configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];
```

**Configuration hierarchy:**

1. appsettings.json
2. appsettings.{Environment}.json (e.g., appsettings.Development.json)
3. Environment variables
4. Command-line arguments

### 3. Middleware Pipeline

ASP.NET Core uses middleware pipeline for request processing.

```csharp
// See: src/UserManagement.API/Program.cs

var app = builder.Build();

// Middleware order is important!
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();      // 1. API documentation
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();  // 2. Redirect HTTP to HTTPS
app.UseAuthentication();    // 3. Who are you? (JWT validation)
app.UseAuthorization();     // 4. What can you do? (Check permissions)
app.MapControllers();       // 5. Route to controllers

app.Run();
```

**Middleware flow:**

```
Request → Swagger → HTTPS Redirect → Authentication → Authorization → Controller → Response
```

---

## Project Structure Deep Dive

This project follows **Clean Architecture** (also called Onion Architecture).

```
UserManagement/
├── src/
│   ├── UserManagement.Core/          # Domain layer (innermost)
│   │   ├── Entities/                 # Domain models
│   │   └── Interfaces/               # Contracts (no implementation)
│   │
│   ├── UserManagement.Application/   # Business logic layer
│   │   ├── DTOs/                     # Data Transfer Objects
│   │   └── Services/                 # Business logic
│   │
│   ├── UserManagement.Infrastructure/ # External concerns layer
│   │   ├── Data/                     # Database context
│   │   ├── Repositories/             # Data access implementation
│   │   └── Security/                 # Security services
│   │
│   ├── UserManagement.API/           # Presentation layer (outermost)
│   │   └── Controllers/              # HTTP endpoints
│   │
│   └── UserManagement.CLI/           # Alternative presentation
│
└── tests/                            # Test projects
    ├── UserManagement.Tests.Unit/
    └── UserManagement.Tests.Integration/
```

### Why This Structure?

**Dependency Direction:**

```
API → Application → Core ← Infrastructure
                     ↑
                  (center)
```

- **Core** has NO dependencies (pure business logic)
- **Application** depends only on Core
- **Infrastructure** implements Core interfaces
- **API** orchestrates everything

**Benefits:**

- Easy to test (mock interfaces)
- Easy to swap implementations (e.g., change from SQLite to PostgreSQL)
- Business logic is isolated and portable

---

## Step-by-Step Code Walkthrough

### Example 1: Creating a User (Full Request Flow)

Let's trace a `POST /api/users` request through the entire application.

#### Step 1: Request Arrives at Controller

```csharp
// File: src/UserManagement.API/Controllers/UsersController.cs

[HttpPost]
[AllowAnonymous]  // No authentication required
public async Task<IActionResult> Create(
    [FromBody] CreateUserDto createUserDto,  // JSON → Object
    CancellationToken cancellationToken)
{
    try
    {
        // Call service layer
        var user = await _userService.CreateAsync(createUserDto, cancellationToken);

        // Return 201 Created with Location header
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }
    catch (InvalidOperationException ex)
    {
        return BadRequest(new { message = ex.Message });
    }
}
```

**What's happening:**

- `[HttpPost]`: This method handles POST requests
- `[FromBody]`: Deserialize JSON request body to `CreateUserDto`
- `CreatedAtAction`: Returns 201 with location header pointing to `GET /api/users/{id}`
- `catch`: Convert exceptions to HTTP error responses

**HTTP Request Example:**

```http
POST /api/users
Content-Type: application/json

{
  "username": "johndoe",
  "email": "john@example.com",
  "password": "SecurePass123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

#### Step 2: Service Layer Processes Business Logic

```csharp
// File: src/UserManagement.Application/Services/UserService.cs

public async Task<UserDto> CreateAsync(
    CreateUserDto createUserDto,
    CancellationToken cancellationToken = default)
{
    // Business Rule 1: Check if username already exists
    var existingUser = await _userRepository.GetByUsernameAsync(
        createUserDto.Username,
        cancellationToken
    );
    if (existingUser != null)
    {
        throw new InvalidOperationException(
            $"Username '{createUserDto.Username}' already exists."
        );
    }

    // Business Rule 2: Check if email already exists
    existingUser = await _userRepository.GetByEmailAsync(
        createUserDto.Email,
        cancellationToken
    );
    if (existingUser != null)
    {
        throw new InvalidOperationException(
            $"Email '{createUserDto.Email}' already exists."
        );
    }

    // Create domain entity
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

    // Save to database
    var createdUser = await _userRepository.CreateAsync(user, cancellationToken);

    // Convert to DTO (don't expose domain entity to API layer)
    return MapToDto(createdUser);
}
```

**Key Concepts:**

- **Validation**: Business rules enforced here
- **Password Hashing**: Never store plain passwords
- **Domain Entity → DTO**: Don't expose internal structure
- **Async/Await**: Non-blocking I/O operations

#### Step 3: Repository Saves to Database

```csharp
// File: src/UserManagement.Infrastructure/Repositories/UserRepository.cs

public async Task<User> CreateAsync(User user, CancellationToken cancellationToken = default)
{
    _context.Users.Add(user);  // Add to DbContext
    await _context.SaveChangesAsync(cancellationToken);  // Persist to DB
    return user;
}
```

**What's happening:**

- `_context.Users.Add(user)`: Tracks entity in memory
- `SaveChangesAsync()`: Generates SQL and executes transaction
- Entity Framework generates SQL automatically

**Generated SQL (roughly):**

```sql
INSERT INTO Users (Id, Username, Email, PasswordHash, FirstName, LastName, CreatedAt, IsActive)
VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7);
```

#### Step 4: Response Returns to Client

```http
HTTP/1.1 201 Created
Location: /api/users/3fa85f64-5717-4562-b3fc-2c963f66afa6
Content-Type: application/json

{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "username": "johndoe",
  "email": "john@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "createdAt": "2025-10-28T19:30:00Z",
  "updatedAt": null,
  "isActive": true
}
```

---

### Example 2: JWT Authentication Flow

#### Step 1: Login Request

```csharp
// File: src/UserManagement.API/Controllers/AuthController.cs

[HttpPost("login")]
public async Task<IActionResult> Login(
    [FromBody] LoginDto loginDto,
    CancellationToken cancellationToken)
{
    var result = await _authService.LoginAsync(loginDto, cancellationToken);

    if (result == null)
    {
        return Unauthorized(new { message = "Invalid username or password" });
    }

    return Ok(result);  // Returns token + user info
}
```

#### Step 2: Validate Credentials and Generate Token

```csharp
// File: src/UserManagement.Application/Services/AuthService.cs

public async Task<AuthResultDto?> LoginAsync(
    LoginDto loginDto,
    CancellationToken cancellationToken = default)
{
    // 1. Find user by username
    var user = await _userRepository.GetByUsernameAsync(
        loginDto.Username,
        cancellationToken
    );

    // 2. Check if user exists and is active
    if (user == null || !user.IsActive)
    {
        return null;  // Don't reveal which check failed (security)
    }

    // 3. Verify password
    if (!_passwordHasher.VerifyPassword(loginDto.Password, user.PasswordHash))
    {
        return null;
    }

    // 4. Generate JWT token
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
```

#### Step 3: Generate JWT Token

```csharp
// File: src/UserManagement.Infrastructure/Security/TokenService.cs

public string GenerateToken(User user)
{
    var jwtSettings = _configuration.GetSection("JwtSettings");
    var secretKey = jwtSettings["SecretKey"]
        ?? throw new InvalidOperationException("JWT SecretKey not configured");

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    // Claims = information embedded in token
    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
    };

    // Create token
    var token = new JwtSecurityToken(
        issuer: jwtSettings["Issuer"],
        audience: jwtSettings["Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(60),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

**JWT Token Structure:**

```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c
└─────────── Header ───────────┘ └──────────────────── Payload ────────────────────┘ └─────────── Signature ──────────┘
```

#### Step 4: Using the Token

```http
GET /api/users
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Authentication Middleware validates:**

1. Token signature is valid
2. Token hasn't expired
3. Issuer and audience match
4. Extract claims and set `User` property on HttpContext

```csharp
// In controller, you can access authenticated user:
var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
```

---

### Example 3: Entity Framework Core

#### DbContext Definition

```csharp
// File: src/UserManagement.Infrastructure/Data/ApplicationDbContext.cs

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSet represents a table
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity (table structure)
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);  // Primary key

            // Unique indexes
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();

            // Column constraints
            entity.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.PasswordHash).IsRequired();
        });
    }
}
```

#### Query Examples

```csharp
// Get by ID
var user = await _context.Users
    .AsNoTracking()  // Read-only query (better performance)
    .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

// Get all with filtering and ordering
var activeUsers = await _context.Users
    .Where(u => u.IsActive)
    .OrderBy(u => u.Username)
    .ToListAsync(cancellationToken);

// Update
_context.Users.Update(user);
await _context.SaveChangesAsync(cancellationToken);

// Delete
_context.Users.Remove(user);
await _context.SaveChangesAsync(cancellationToken);
```

---

## Key Differences from Other Languages

### C# vs Java

| Feature        | C#                                   | Java                                        |
| -------------- | ------------------------------------ | ------------------------------------------- |
| Properties     | `public string Name { get; set; }`   | `getName()` / `setName()`                   |
| Type inference | `var list = new List<string>();`     | `List<String> list = new ArrayList<>();`    |
| Async/Await    | Built-in: `async Task<T>`            | CompletableFuture or libraries              |
| LINQ           | Built-in: `list.Where(x => x > 5)`   | Streams: `list.stream().filter(x -> x > 5)` |
| Null safety    | `string?` (nullable reference types) | `@Nullable` (annotations)                   |
| Records        | `public record Person(string Name);` | `record Person(String name) {}` (Java 16+)  |

### C# vs JavaScript/TypeScript

| Feature       | C#                        | JavaScript/TypeScript               |
| ------------- | ------------------------- | ----------------------------------- |
| Typing        | Statically typed          | JS: Dynamic, TS: Static             |
| Async         | `async`/`await` with Task | `async`/`await` with Promise        |
| Classes       | Full OOP with interfaces  | Prototype-based (TS has interfaces) |
| Modules       | Namespaces + using        | import/export                       |
| DI            | Built-in container        | Libraries (e.g., tsyringe)          |
| Null checking | `?.` `??` `??=`           | `?.` `??` `??=` (similar!)          |

### C# vs Python

| Feature     | C#                 | Python                   |
| ----------- | ------------------ | ------------------------ |
| Typing      | Static, compiled   | Dynamic, interpreted     |
| Indentation | Braces `{ }`       | Significant whitespace   |
| Properties  | Auto-properties    | `@property` decorator    |
| Async       | `async Task`       | `async def` with asyncio |
| DI          | Built-in framework | Manual or libraries      |
| Performance | Compiled, fast     | Interpreted, slower      |

---

## Common Patterns in This Project

### 1. Repository Pattern

**Purpose:** Abstract data access logic

```csharp
// Interface (contract)
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User> CreateAsync(User user, CancellationToken cancellationToken = default);
    // ... other methods
}

// Implementation
public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }
}
```

**Benefits:**

- Easy to test (mock the interface)
- Easy to swap implementations (SQLite → PostgreSQL)
- Centralizes data access logic

### 2. DTO Pattern

**Purpose:** Control what data is exposed to API consumers

```csharp
// Domain Entity (internal, full data)
public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }  // ⚠️ Sensitive!
    // ...
}

// DTO (public, safe to expose)
public record UserDto(
    Guid Id,
    string Username,
    string Email,
    // Note: NO PasswordHash!
    // ...
);
```

**Why?**

- Security: Don't expose password hashes
- Versioning: Can change internal structure without breaking API
- Validation: Different validation rules for create vs update

### 3. Service Layer Pattern

**Purpose:** Encapsulate business logic

```csharp
public class UserService
{
    private readonly IUserRepository _repository;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(IUserRepository repository, IPasswordHasher passwordHasher)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserDto> CreateAsync(CreateUserDto dto, CancellationToken ct = default)
    {
        // Business logic here
        // - Validation
        // - Password hashing
        // - Entity creation
        // - Saving to DB
    }
}
```

**Benefits:**

- Testable (mock dependencies)
- Reusable (can be called from API, CLI, background jobs, etc.)
- Single Responsibility: Each service handles one domain concept

### 4. Options Pattern

**Purpose:** Strongly-typed configuration

Instead of:

```csharp
var secretKey = configuration["JwtSettings:SecretKey"];  // String, error-prone
```

You can use:

```csharp
public class JwtSettings
{
    public string SecretKey { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int ExpiryMinutes { get; set; }
}

// Register in Program.cs
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

// Inject in service
public class TokenService
{
    private readonly JwtSettings _settings;

    public TokenService(IOptions<JwtSettings> options)
    {
        _settings = options.Value;
    }
}
```

---

## Testing in .NET

### Unit Tests with xUnit

```csharp
// File: tests/UserManagement.Tests.Unit/Services/UserServiceTests.cs

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _mockRepository;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        // Setup mocks
        _mockRepository = new Mock<IUserRepository>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();

        // Create service with mocked dependencies
        _userService = new UserService(
            _mockRepository.Object,
            _mockPasswordHasher.Object
        );
    }

    [Fact]  // Test method
    public async Task GetByIdAsync_ExistingUser_ReturnsUserDto()
    {
        // Arrange: Setup test data and mocks
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com"
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act: Execute the method being tested
        var result = await _userService.GetByIdAsync(userId);

        // Assert: Verify the results
        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
        result.Username.Should().Be("testuser");
    }
}
```

**Key Testing Libraries:**

- **xUnit**: Test framework
- **Moq**: Mocking framework
- **FluentAssertions**: Readable assertions

### Integration Tests

```csharp
// File: tests/UserManagement.Tests.Integration/ApiTests/UsersControllerTests.cs

public class UsersControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public UsersControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();  // Test HTTP client
    }

    [Fact]
    public async Task CreateUser_ValidData_ReturnsCreatedUser()
    {
        // Arrange
        var createUserDto = new CreateUserDto(
            "newuser",
            "new@example.com",
            "Password123!",
            "New",
            "User"
        );

        // Act: Make HTTP request
        var response = await _client.PostAsJsonAsync("/api/users", createUserDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        user.Should().NotBeNull();
        user!.Username.Should().Be("newuser");
    }
}
```

---

## Next Steps

### 1. Experiment with the Code

Try these modifications:

**Easy:**

- Add a new property to `User` entity (e.g., `PhoneNumber`)
- Add a new endpoint to get users by email
- Change JWT token expiry time

**Medium:**

- Add user roles (Admin, User)
- Add role-based authorization to endpoints
- Add email validation with regex

**Hard:**

- Add refresh tokens
- Add pagination to `GET /api/users`
- Add user profile pictures with file upload

### 2. Learn More

**Official Resources:**

- [Microsoft Learn - C#](https://learn.microsoft.com/en-us/dotnet/csharp/)
- [Microsoft Learn - ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core Docs](https://learn.microsoft.com/en-us/ef/core/)

**Recommended Books:**

- "C# in Depth" by Jon Skeet
- "Pro ASP.NET Core" by Adam Freeman

### 3. Practice Projects

Build variations of this project:

1. **Blog API**: Posts, comments, tags
2. **E-commerce API**: Products, orders, cart
3. **Task Manager**: Projects, tasks, assignments

---

## Quick Reference

### Common Commands

```bash
# Build project
dotnet build

# Run project
dotnet run --project src/UserManagement.API

# Run tests
dotnet test

# Add package
dotnet add package PackageName

# Create new project
dotnet new webapi -n MyProject

# Entity Framework migrations
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Common Attributes

```csharp
// Controller/Action attributes
[ApiController]         // Enables API-specific features
[Route("api/[controller]")]  // URL routing
[HttpGet]              // HTTP verb
[Authorize]            // Requires authentication
[AllowAnonymous]       // Allows unauthenticated access

// Validation attributes
[Required]
[MaxLength(50)]
[EmailAddress]
[Range(1, 100)]

// Parameter binding
[FromBody]             // Bind from request body
[FromRoute]            // Bind from URL
[FromQuery]            // Bind from query string
```

### Useful Code Snippets

```csharp
// Using statement (auto-dispose resources)
using (var scope = serviceProvider.CreateScope())
{
    var service = scope.ServiceProvider.GetRequiredService<MyService>();
    // scope.Dispose() called automatically
}

// Pattern matching
var result = value switch
{
    null => "Value is null",
    0 => "Value is zero",
    > 0 => "Value is positive",
    _ => "Value is negative"
};

// String interpolation
var message = $"User {username} created at {DateTime.Now}";

// Collection initialization
var list = new List<string> { "one", "two", "three" };
var dict = new Dictionary<string, int>
{
    ["one"] = 1,
    ["two"] = 2
};
```

---

## Troubleshooting Common Issues

### 1. "Cannot resolve service" error

**Problem:** DI container can't find a registered service

**Solution:** Check `Program.cs` - make sure service is registered:

```csharp
builder.Services.AddScoped<IMyService, MyService>();
```

### 2. "Object reference not set to an instance of an object" (NullReferenceException)

**Problem:** Trying to use a null object

**Solution:** Use null-conditional operators:

```csharp
var length = username?.Length ?? 0;  // Safe
```

### 3. Database not found

**Problem:** Database file doesn't exist

**Solution:** Check that `EnsureCreated()` is called in `Program.cs`:

```csharp
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated();
}
```

### 4. JWT authentication fails

**Problem:** Token validation fails

**Check:**

- Same secret key in token generation and validation
- Token hasn't expired
- Issuer and audience match
- Token format: `Bearer eyJ...`

---

## Summary

You've learned:

- ✅ C# syntax and modern features (records, null safety, async/await)
- ✅ .NET Core dependency injection and middleware
- ✅ Clean Architecture pattern
- ✅ Entity Framework Core for data access
- ✅ JWT authentication
- ✅ Testing with xUnit
- ✅ Repository and service patterns

**Next:** Start modifying the code! The best way to learn is by doing.

Happy coding! 🚀
