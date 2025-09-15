using Antecipacao.Domain.Entities;
using Antecipacao.Domain.Interfaces;
using Antecipacao.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Antecipacao.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AntecipacaoDbContext _context;

        public UserRepository(AntecipacaoDbContext context)
        {
            _context = context;
        }

        // Buscar User por Username
        public async Task<User?> ObterPorUsernameAsync(string username)
        {
            return await _context.Users
                .Include(u => u.RefreshTokens)
                .SingleOrDefaultAsync(u => u.Username == username);
        }

        // Atualizar User
        public Task AtualizarAsync(User user)
        {
            _context.Users.Update(user);
            _context.SaveChanges();
            return Task.CompletedTask;
        }

        // Buscar User por RefreshToken
        public async Task<User?> ObterPorRefreshTokenAsync(string token)
        {
            return await _context.Users
                .Include(u => u.RefreshTokens)
                .SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));
        }

        // Criar novo User
        public async Task<User> CriarAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }
    }
}
