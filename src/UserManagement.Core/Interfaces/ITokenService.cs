using UserManagement.Core.Entities;

namespace UserManagement.Core.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}
