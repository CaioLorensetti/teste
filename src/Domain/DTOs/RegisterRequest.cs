using Antecipacao.Domain.Entities;

namespace Antecipacao.Domain.DTOs
{
    public class RegisterRequest
    {
        public string Username { get; set; } = string.Empty; // Email válido e único
        public string Password { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.User; // Padrão para User
    }
}
