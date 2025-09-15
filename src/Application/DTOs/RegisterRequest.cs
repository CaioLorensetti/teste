namespace Antecipacao.Application.DTOs
{
    public class RegisterRequest
    {
        public string Username { get; set; } = string.Empty; // Email válido e único
        public string Password { get; set; } = string.Empty;
    }
}
