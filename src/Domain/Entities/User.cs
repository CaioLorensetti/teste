namespace Antecipacao.Domain.Entities
{
    public enum UserRole
    {
        Admin = 0,
        User = 1
    }

    public class User
    {
        public long Id { get; set; }
        public string Username { get; set; } // Email válido e único
        public string PasswordHash { get; set; }
        public string Salt { get; set; }
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
