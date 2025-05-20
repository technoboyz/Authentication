using System.Security.Claims;

namespace Techno.Authentication.Interfaces
{
    public interface IJwtBearerService
    {
        Task<string> CreateTokenAsync(string username, IList<Claim>? claims = null, string? issuer = null, string? audience = null, DateTime? absoluteExpiration = null);
        Task<ClaimsPrincipal> ValidateTokenAsync(string token, bool validateLifetime = true);
        Task<string> RefreshTokenAsync(string token, bool validateLifetime, DateTime? absoluteExpiration = null);
    }
}
