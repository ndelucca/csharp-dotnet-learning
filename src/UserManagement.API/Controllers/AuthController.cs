using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.DTOs;
using UserManagement.Application.Services;

namespace UserManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginDto loginDto,
        CancellationToken cancellationToken
    )
    {
        _logger.LogInformation("Login attempt for username: {Username}", loginDto.Username);

        var result = await _authService.LoginAsync(loginDto, cancellationToken);

        if (result == null)
        {
            _logger.LogWarning("Failed login attempt for username: {Username}", loginDto.Username);
            return Unauthorized(new { message = "Invalid username or password" });
        }

        _logger.LogInformation("Successful login for username: {Username}", loginDto.Username);
        return Ok(result);
    }
}
