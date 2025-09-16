using Antecipacao.Domain.DTOs;
using Antecipacao.Domain.Entities;

namespace Antecipacao.Domain.Interfaces
{
    public interface IAuthenticationService
    {
        Task<AuthenticationResponse> RefreshTokenAsync(string refreshToken, string ipAddress);
        Task<bool> RevokeTokenAsync(string token, string ipAddress);
        Task<bool> IsTokenValidAsync(string token);
        RefreshToken GenerateRefreshToken(string ipAddress);

        Task<AuthenticationResponse?> AuthenticateAsync(LoginRequest request, string ipAddress);
        Task RegisterAsync(RegisterRequest request);
    }
}
