# New Domain Entity

Follow this order (Core → Infrastructure → Application → API):

1. **Entity** in `src/UserManagement.Core/Entities/`:
```csharp
public class EntityName
{
    public Guid Id { get; set; }
    // properties
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

2. **Repository interface** in `src/UserManagement.Core/Interfaces/`:
```csharp
public interface IEntityNameRepository
{
    Task<EntityName?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<EntityName>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<EntityName> CreateAsync(EntityName entity, CancellationToken cancellationToken = default);
    Task<EntityName> UpdateAsync(EntityName entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
```

3. **DbSet** in `src/UserManagement.Infrastructure/Data/ApplicationDbContext.cs`

4. **Repository implementation** in `src/UserManagement.Infrastructure/Repositories/`

5. **Register in DI** in `src/UserManagement.API/Program.cs`:
```csharp
builder.Services.AddScoped<IEntityNameRepository, EntityNameRepository>();
```

6. **DTOs** in `src/UserManagement.Application/DTOs/`

7. **Service** in `src/UserManagement.Application/Services/`

8. **Controller** in `src/UserManagement.API/Controllers/`

9. **Unit + Integration tests**
