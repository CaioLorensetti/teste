using Antecipacao.Domain.Entities;

namespace Antecipacao.Domain.DTOs
{
    public class AuthenticationResponse
    {
        public string Username { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
