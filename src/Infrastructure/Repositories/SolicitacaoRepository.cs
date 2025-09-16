using Antecipacao.Domain.Entities;
using Antecipacao.Domain.Interfaces;
using Antecipacao.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Antecipacao.Infrastructure.Repositories
{
    public class SolicitacaoRepository : ISolicitacaoRepository
    {
        private readonly AntecipacaoDbContext _context;

        public SolicitacaoRepository(AntecipacaoDbContext context)
        {
            _context = context;
        }

        public async Task<SolicitacaoAntecipacao?> ObterPorIdAsync(long id)
        {
            return await _context.Solicitacoes.FindAsync(id);
        }

        public async Task<SolicitacaoAntecipacao?> ObterPorGuidIdAsync(Guid guidId)
        {
            return await _context.Solicitacoes
                .FirstOrDefaultAsync(s => s.GuidId == guidId);
        }

        public async Task<IEnumerable<SolicitacaoAntecipacao>> ListarPorCreatorAsync(long creatorId)
        {
            return await _context.Solicitacoes
                .Where(s => s.CreatorId == creatorId)
                .OrderByDescending(s => s.DataSolicitacao)
                .ToListAsync();
        }

        public async Task<IEnumerable<SolicitacaoAntecipacao>> ListarTodasAsync()
        {
            return await _context.Solicitacoes
                .OrderByDescending(s => s.DataSolicitacao)
                .ToListAsync();
        }

        public async Task<bool> ExisteSolicitacaoPendenteAsync(long creatorId)
        {
            return await _context.Solicitacoes
                .AnyAsync(s => s.CreatorId == creatorId && s.Status == Domain.Enums.StatusSolicitacao.Pendente);
        }

        public Task<SolicitacaoAntecipacao> AdicionarAsync(SolicitacaoAntecipacao solicitacao)
        {
            _context.Solicitacoes.Add(solicitacao);
            return Task.FromResult(solicitacao);
        }

        public Task AtualizarAsync(SolicitacaoAntecipacao solicitacao)
        {
            _context.Solicitacoes.Update(solicitacao);
            return Task.CompletedTask;
        }

        public Task RemoverAsync(SolicitacaoAntecipacao solicitacao)
        {
            _context.Solicitacoes.Remove(solicitacao);
            return Task.CompletedTask;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
