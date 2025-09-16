using Antecipacao.Domain.DTOs;
using Antecipacao.Domain.Entities;
using Antecipacao.Domain.Interfaces;

namespace Antecipacao.Application.Services
{
    public class AntecipacaoService : IAntecipacaoService
    {
        private readonly ISolicitacaoRepository _repository;

        public AntecipacaoService(ISolicitacaoRepository repository)
        {
            _repository = repository;
        }

        public async Task<MinhaSolicitacaoResponseDto> CriarSolicitacaoAsync(CriarSolicitacaoDto dto)
        {
            // Validar valor mínimo
            if (dto.ValorSolicitado <= 100)
                throw new ArgumentException("Valor deve ser maior que R$ 100,00");

            // Verificar se já existe solicitação pendente
            var existePendente = await _repository.ExisteSolicitacaoPendenteAsync(dto.CreatorId);
            if (existePendente)
                throw new InvalidOperationException("Creator já possui uma solicitação pendente");

            // Criar solicitação
            var solicitacao = new SolicitacaoAntecipacao(
                dto.CreatorId,
                dto.ValorSolicitado,
                dto.DataSolicitacao
            );

            await _repository.AdicionarAsync(solicitacao);
            await _repository.SaveChangesAsync();

            return MapToResponseDto(solicitacao);
        }

        public async Task<IEnumerable<MinhaSolicitacaoResponseDto>> ListarPorCreatorAsync(long creatorId)
        {
            var solicitacoes = await _repository.ListarPorCreatorAsync(creatorId);
            return solicitacoes.Select(MapToResponseDto);
        }

        public async Task<MinhaSolicitacaoResponseDto> AprovarAsync(Guid guidId)
        {
            var solicitacao = await _repository.ObterPorGuidIdAsync(guidId);
            if (solicitacao == null)
                throw new ArgumentException("Solicitação não encontrada");

            solicitacao.Aprovar();
            await _repository.SaveChangesAsync();

            return MapToResponseDto(solicitacao);
        }

        public async Task<MinhaSolicitacaoResponseDto> RecusarAsync(Guid guidId)
        {
            var solicitacao = await _repository.ObterPorGuidIdAsync(guidId);
            if (solicitacao == null)
                throw new ArgumentException("Solicitação não encontrada");

            solicitacao.Recusar();
            await _repository.SaveChangesAsync();

            return MapToResponseDto(solicitacao);
        }

        public Task<SimulacaoDto> SimularAntecipacaoAsync(decimal valor)
        {
            if (valor <= 100)
                throw new ArgumentException("Valor deve ser maior que R$ 100,00");

            var taxa = 0.05m;
            var valorLiquido = valor - (valor * taxa);

            return Task.FromResult(new SimulacaoDto
            {
                ValorSolicitado = valor,
                TaxaAplicada = taxa,
                ValorLiquido = valorLiquido
            });
        }

        private static MinhaSolicitacaoResponseDto MapToResponseDto(SolicitacaoAntecipacao solicitacao)
        {
            return new MinhaSolicitacaoResponseDto
            {
                GuidId = solicitacao.GuidId,
                ValorSolicitado = solicitacao.ValorSolicitado,
                TaxaAplicada = solicitacao.TaxaAplicada,
                ValorLiquido = solicitacao.ValorLiquido,
                DataSolicitacao = solicitacao.DataSolicitacao,
                Status = solicitacao.Status.ToString(),
                DataAprovacao = solicitacao.DataAprovacao,
                DataRecusa = solicitacao.DataRecusa
            };
        }
    }
}
