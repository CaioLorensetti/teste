using Antecipacao.Domain.Entities;
using Antecipacao.Domain.Interfaces;
using Antecipacao.Domain.ValueObjects;
using Antecipacao.Application.Services;

namespace Antecipacao.Infrastructure.Data
{
    public class SeedDataService
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtSettings _jwtSettings;

        public SeedDataService(IUserRepository userRepository, JwtSettings jwtSettings)
        {
            _userRepository = userRepository;
            _jwtSettings = jwtSettings;
        }

        public async Task SeedAdminUserAsync()
        {
            // Verificar se já existe um usuário admin
            var existingAdmin = await _userRepository.ObterPorUsernameAsync("admin@antecipacao.com");
            if (existingAdmin != null)
                return;

            // Criar usuário admin
            var adminPassword = "Admin@123";
            var hashPassword = BCrypt.Net.BCrypt.HashPassword(adminPassword);
            var salt = AuthenticationService.ExtraiSaltDoHash(hashPassword);

            var adminUser = new User
            {
                Fullname = "Administrador",
                Username = "admin@antecipacao.com",
                PasswordHash = hashPassword,
                Salt = salt,
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.CriarAsync(adminUser);
        }
    }
}
