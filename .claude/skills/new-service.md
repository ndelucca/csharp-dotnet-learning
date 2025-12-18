# New Application Service

1. **Service class** in `src/UserManagement.Application/Services/`:
```csharp
using UserManagement.Core.Interfaces;

namespace UserManagement.Application.Services;

public class NewService
{
    private readonly IRepository _repository;

    public NewService(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<ResultDto> DoSomethingAsync(InputDto input, CancellationToken cancellationToken = default)
    {
        // Business logic
    }
}
```

2. **Register in DI** in `src/UserManagement.API/Program.cs`:
```csharp
builder.Services.AddScoped<NewService>();
```

3. **Unit tests** in `tests/UserManagement.Tests.Unit/Services/`
