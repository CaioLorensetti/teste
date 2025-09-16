using Antecipacao.Domain.DTOs;

namespace Antecipacao.Domain.Interfaces
{
    public interface IAntecipacaoService
    {
        Task<MinhaSolicitacaoResponseDto> CriarSolicitacaoAsync(CriarSolicitacaoDto dto);
        Task<IEnumerable<MinhaSolicitacaoResponseDto>> ListarPorCreatorAsync(long creatorId);
        Task<MinhaSolicitacaoResponseDto> AprovarAsync(Guid guidId);
        Task<MinhaSolicitacaoResponseDto> RecusarAsync(Guid guidId);
        Task<SimulacaoDto> SimularAntecipacaoAsync(decimal valor);
    }
}
