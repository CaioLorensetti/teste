using Antecipacao.Domain.Entities;

namespace Antecipacao.Domain.Interfaces
{
    public interface IAuthenticationService
    {
        Task<User?> AuthenticateAsync(string username, string password);
        Task<User> RegisterAsync(string username, string password, string role = "User");
        Task<string> GenerateJwtTokenAsync(User user);
        Task<RefreshToken> GenerateRefreshTokenAsync(User user, string ipAddress);
        Task<RefreshToken?> GetRefreshTokenAsync(string token);
        Task RevokeTokenAsync(string token, string ipAddress, string reason = "Revoked");
        Task<bool> IsTokenValidAsync(string token);
    }
}
