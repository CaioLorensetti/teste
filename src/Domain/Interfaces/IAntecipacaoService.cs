using Antecipacao.Domain.DTOs;

namespace Antecipacao.Domain.Interfaces
{
    public interface IAntecipacaoService
    {
        Task<SolicitacaoResponseDto> CriarSolicitacaoAsync(CriarSolicitacaoDto dto);
        Task<IEnumerable<SolicitacaoResponseDto>> ListarPorCreatorAsync(long creatorId);
        Task<SolicitacaoResponseDto> AprovarAsync(Guid guidId);
        Task<SolicitacaoResponseDto> RecusarAsync(Guid guidId);
        Task<SimulacaoDto> SimularAntecipacaoAsync(decimal valor);
    }
}
