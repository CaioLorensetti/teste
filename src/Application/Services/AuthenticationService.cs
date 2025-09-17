using Antecipacao.Domain.Interfaces;
using Antecipacao.Domain.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Antecipacao.Domain.DTOs;
using Antecipacao.Domain.ValueObjects;
using System.Security.Cryptography;
using System.Security;

namespace Antecipacao.Application.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly JwtSettings _jwtSettings;

        public AuthenticationService(JwtSettings jwtSettings, IConfiguration configuration, IUserRepository userRepository)
        {
            _jwtSettings = jwtSettings;
            _configuration = configuration;
            _userRepository = userRepository;
        }

        public async Task<AuthenticationResponse?> AuthenticateAsync(LoginRequest request, string ipAddress)
        {
            var user = await _userRepository.ObterPorUsernameAsync(request.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return null;

            var accessToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken(ipAddress);

            user.RefreshTokens.Add(refreshToken);

            // Remover tokens antigos inativos
            RemoveOldRefreshTokens(user);

            await _userRepository.AtualizarAsync(user);

            return new AuthenticationResponse
            {
                Username = user.Username,
                Role = user.Role,
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token
            };
        }


        public async Task<AuthenticationResponse> RefreshTokenAsync(string refreshToken, string ipAddress)
        {
            var user = await GetUserByRefreshToken(refreshToken);
            var token = user.RefreshTokens.Single(x => x.Token == refreshToken);
            if (token == null || !token.IsActive)
                throw new SecurityException("Invalid token");

            if (token.IsRevoked)
            {
                // Revogar todos os tokens descendentes em caso de tentativa de reuso
                RevokeDescendantRefreshTokens(token, user, ipAddress,
                    $"Attempted reuse of revoked ancestor token: {token}");
                await _userRepository.AtualizarAsync(user);
            }

            if (!token.IsActive)
                throw new SecurityException("Invalid token");

            // Substituir token antigo por novo
            var newRefreshToken = RotateRefreshToken(token, ipAddress);
            user.RefreshTokens.Add(newRefreshToken);

            // Remover tokens antigos inativos
            RemoveOldRefreshTokens(user);

            await _userRepository.AtualizarAsync(user);

            // Gerar novo access token
            var accessToken = GenerateJwtToken(user);

            return new AuthenticationResponse
            {
                Username = user.Username,
                Role = user.Role,
                AccessToken = accessToken,
                RefreshToken = newRefreshToken.Token
            };
        }

        public async Task RegisterAsync(RegisterRequest request)
        {
            if (await _userRepository.ObterPorUsernameAsync(request.Username) != null)
                throw new ApplicationException("Usuário já existe.");

            var hashPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
            var salt = ExtraiSaltDoHash(hashPassword);
            var user = new User
            {
                Fullname = request.Fullname,
                Username = request.Username,
                PasswordHash = hashPassword,
                Salt = salt,
                CreatedAt = DateTime.UtcNow,
                Role = UserRole.User
            };

            await _userRepository.CriarAsync(user);
        }

        public static string ExtraiSaltDoHash(string bcryptHash)
        {
            // O salt são os primeiros 29 caracteres do hash
            return bcryptHash.Substring(0, 29);
        }

        public RefreshToken GenerateRefreshToken(string ipAddress)
        {
            return new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                Created = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private RefreshToken RotateRefreshToken(RefreshToken refreshToken, string ipAddress)
        {
            var newRefreshToken = GenerateRefreshToken(ipAddress);
            RevokeRefreshToken(refreshToken, ipAddress, "Replaced by new token", newRefreshToken.Token);
            return newRefreshToken;
        }

        public async Task<bool> RevokeTokenAsync(string token, string ipAddress)
        {
            var user = await GetUserByRefreshToken(token);
            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

            if (!refreshToken.IsActive)
                return false;

            // Revogar token e descendentes
            RevokeRefreshToken(refreshToken, ipAddress, "Revogar token para logout");
            await _userRepository.AtualizarAsync(user);

            return true;
        }

        private void RevokeRefreshToken(RefreshToken token, string ipAddress, string reason = null,
            string replacedByToken = null)
        {
            token.Revoked = DateTime.UtcNow;
            token.RevokedByIp = ipAddress;
            token.ReasonRevoked = reason;
            token.ReplacedByToken = replacedByToken;
        }

        public async Task<bool> IsTokenValidAsync(string token)
        {
            var user = await _userRepository.ObterPorRefreshTokenAsync(token);
            if (user == null) return false;

            var refreshToken = user.RefreshTokens.SingleOrDefault(t => t.Token == token);
            return refreshToken != null && refreshToken.IsActive;
        }

        private void RevokeDescendantRefreshTokens(RefreshToken refreshToken, User user,
            string ipAddress, string reason)
        {
            if (!string.IsNullOrEmpty(refreshToken.ReplacedByToken))
            {
                var childToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken.ReplacedByToken);
                if (childToken != null && childToken.IsActive)
                    RevokeRefreshToken(childToken, ipAddress, reason);
                else
                    RevokeDescendantRefreshTokens(childToken, user, ipAddress, reason);
            }
        }

        private async Task<User> GetUserByRefreshToken(string token)
        {
            var user = await _userRepository.ObterPorRefreshTokenAsync(token);

            if (user == null)
                throw new SecurityException("Token inválido");

            return user;
        }

        private void RemoveOldRefreshTokens(User user)
        {
            // Remover tokens inativos antigos (mais de 2 dias depois de expirados)
            user.RefreshTokens.RemoveAll(x =>
                !x.IsActive &&
                x.Created.AddDays(_jwtSettings.RefreshTokenExpirationDays + 2) <= DateTime.UtcNow);
        }
    }
}
