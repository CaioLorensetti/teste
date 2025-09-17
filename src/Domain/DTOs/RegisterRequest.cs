using Antecipacao.Domain.Entities;

namespace Antecipacao.Domain.DTOs
{
    public class RegisterRequest
    {
        public string Fullname { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
