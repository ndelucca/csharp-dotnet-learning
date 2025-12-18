# New API Endpoint

Follow this layered approach:

1. **DTO** (if needed) in `src/UserManagement.Application/DTOs/`:
   - Use records: `public record XxxDto(...);`

2. **Service method** in `src/UserManagement.Application/Services/`:
   - Inject interfaces from Core via constructor
   - Return DTOs, accept DTOs as parameters
   - Handle business logic and validation

3. **Controller action** in `src/UserManagement.API/Controllers/`:
   - Use `[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpDelete]` attributes
   - Add `[Authorize]` for protected endpoints, `[AllowAnonymous]` for public
   - Add `[ProducesResponseType]` for Swagger documentation
   - Accept `CancellationToken cancellationToken` as last parameter
   - Return `IActionResult` with appropriate status codes

4. **Unit test** in `tests/UserManagement.Tests.Unit/Services/`

5. **Integration test** in `tests/UserManagement.Tests.Integration/ApiTests/`
