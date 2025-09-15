using Antecipacao.Application.DTOs;
using Antecipacao.Application.Interfaces;
using Antecipacao.Domain.Entities;
using DomainAuthService = Antecipacao.Domain.Interfaces.IAuthenticationService;
using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Antecipacao.Application.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly DomainAuthService _domainAuthService;
        private readonly IConfiguration _configuration;

        public AuthenticationService(DomainAuthService domainAuthService, IConfiguration configuration)
        {
            _domainAuthService = domainAuthService;
            _configuration = configuration;
        }

        public async Task<AuthenticationResponse?> AuthenticateAsync(LoginRequest request, string ipAddress)
        {
            var user = await _domainAuthService.AuthenticateAsync(request.Username, request.Password);
            if (user == null)
                return null;

            var accessToken = await _domainAuthService.GenerateJwtTokenAsync(user);
            var refreshToken = await _domainAuthService.GenerateRefreshTokenAsync(user, ipAddress);

            return new AuthenticationResponse
            {
                Username = user.Username,
                Role = user.Role,
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token
            };
        }

        public async Task RegisterAsync(RegisterRequest request)
        {
            var user = await _domainAuthService.RegisterAsync(request.Username, request.Password);
        }

        public async Task<AuthenticationResponse> RefreshTokenAsync(string refreshToken, string ipAddress)
        {
            var token = await _domainAuthService.GetRefreshTokenAsync(refreshToken);
            if (token == null || !token.IsActive)
                throw new System.Security.SecurityException("Token inválido");

            var accessToken = await _domainAuthService.GenerateJwtTokenAsync(token.User);
            var newRefreshToken = await _domainAuthService.GenerateRefreshTokenAsync(token.User, ipAddress);

            // Revogar o token antigo
            await _domainAuthService.RevokeTokenAsync(refreshToken, ipAddress, "Replaced by new token");

            return new AuthenticationResponse
            {
                Username = token.User.Username,
                Role = token.User.Role,
                AccessToken = accessToken,
                RefreshToken = newRefreshToken.Token
            };
        }

        public async Task RevokeTokenAsync(string refreshToken, string ipAddress)
        {
            await _domainAuthService.RevokeTokenAsync(refreshToken, ipAddress);
        }
    }
}
