using Core.Entities;

namespace Core.Services;

public interface ITokenService
{
    string CreateToken(User user, out DateTime expiresAt);
}
