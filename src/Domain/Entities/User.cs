namespace Antecipacao.Domain.Entities
{
    public class User
    {
        public long Id { get; set; }
        public string Username { get; set; } = string.Empty; // Email válido e único
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public DateTime CreatedAt { get; set; }
        public List<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
