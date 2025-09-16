using Antecipacao.Domain.Entities;

namespace Antecipacao.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> ObterPorUsernameAsync(string username);
        Task AtualizarAsync(User user);
        Task<User?> ObterPorRefreshTokenAsync(string token);
        Task<User> CriarAsync(User user);
    }
}
