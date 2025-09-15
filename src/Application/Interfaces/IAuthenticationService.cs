using Antecipacao.Application.DTOs;

namespace Antecipacao.Application.Interfaces
{
    public interface IAuthenticationService
    {
        Task<AuthenticationResponse?> AuthenticateAsync(LoginRequest request, string ipAddress);
        Task RegisterAsync(RegisterRequest request);
        Task<AuthenticationResponse> RefreshTokenAsync(string refreshToken, string ipAddress);
        Task RevokeTokenAsync(string refreshToken, string ipAddress);
    }
}
