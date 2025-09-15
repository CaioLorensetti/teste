using Antecipacao.Domain.Entities;

namespace Antecipacao.Domain.Interfaces
{
    public interface ISolicitacaoRepository
    {
        Task<SolicitacaoAntecipacao?> ObterPorIdAsync(long id);
        Task<SolicitacaoAntecipacao?> ObterPorGuidIdAsync(Guid guidId);
        Task<IEnumerable<SolicitacaoAntecipacao>> ListarPorCreatorAsync(long creatorId);
        Task<IEnumerable<SolicitacaoAntecipacao>> ListarTodasAsync();
        Task<bool> ExisteSolicitacaoPendenteAsync(long creatorId);
        Task<SolicitacaoAntecipacao> AdicionarAsync(SolicitacaoAntecipacao solicitacao);
        Task AtualizarAsync(SolicitacaoAntecipacao solicitacao);
        Task RemoverAsync(SolicitacaoAntecipacao solicitacao);
        Task<int> SaveChangesAsync();
    }
}
