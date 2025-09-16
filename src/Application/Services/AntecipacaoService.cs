using Antecipacao.Domain.DTOs;
using Antecipacao.Domain.Entities;
using Antecipacao.Domain.Interfaces;

namespace Antecipacao.Application.Services
{
    public class AntecipacaoService : IAntecipacaoService
    {
        private readonly ISolicitacaoRepository _repositorioSolicitacao;
        private const decimal VALOR_MINIMO_SOLICITACAO = 100m;
        private const decimal TAXA_ANTECIPACAO = 0.05m;

        public AntecipacaoService(ISolicitacaoRepository repositorioSolicitacao)
        {
            _repositorioSolicitacao = repositorioSolicitacao;
        }

        public async Task<MinhaSolicitacaoResponseDto> CriarSolicitacaoAsync(CriarSolicitacaoDto dto)
        {
            ValidarValorMinimo(dto.ValorSolicitado);
            await ValidarSolicitacaoPendente(dto.IdCriador);

            var solicitacao = CriarNovaSolicitacao(dto);
            await SalvarSolicitacao(solicitacao);

            return MapearResponseDto(solicitacao);
        }

        public async Task<IEnumerable<MinhaSolicitacaoResponseDto>> ListarPorCreatorAsync(long idCreator)
        {
            var solicitacoes = await _repositorioSolicitacao.ListarPorCreatorAsync(idCreator);
            return solicitacoes.Select(MapearResponseDto);
        }

        public async Task<MinhaSolicitacaoResponseDto> AprovarAsync(Guid guidId)
        {
            var solicitacao = await ObterSolicitacaoPorId(guidId);
            solicitacao.Aprovar();
            await _repositorioSolicitacao.SaveChangesAsync();

            return MapearResponseDto(solicitacao);
        }

        public async Task<MinhaSolicitacaoResponseDto> RecusarAsync(Guid guidId)
        {
            var solicitacao = await ObterSolicitacaoPorId(guidId);
            solicitacao.Recusar();
            await _repositorioSolicitacao.SaveChangesAsync();

            return MapearResponseDto(solicitacao);
        }

        public Task<SimulacaoDto> SimularAntecipacaoAsync(decimal valor)
        {
            ValidarValorMinimo(valor);
            var simulacao = CalcularSimulacao(valor);
            return Task.FromResult(simulacao);
        }

        private void ValidarValorMinimo(decimal valor)
        {
            if (valor <= VALOR_MINIMO_SOLICITACAO)
                throw new ArgumentException($"Valor deve ser maior que R$ {VALOR_MINIMO_SOLICITACAO:F2}");
        }

        private async Task ValidarSolicitacaoPendente(long idCreator)
        {
            var existePendente = await _repositorioSolicitacao.ExisteSolicitacaoPendenteAsync(idCreator);
            if (existePendente)
                throw new InvalidOperationException("Usuário já possui uma solicitação pendente");
        }

        private SolicitacaoAntecipacao CriarNovaSolicitacao(CriarSolicitacaoDto dto)
        {
            return new SolicitacaoAntecipacao(
                dto.IdCriador,
                dto.ValorSolicitado,
                dto.DataSolicitacao
            );
        }

        private async Task SalvarSolicitacao(SolicitacaoAntecipacao solicitacao)
        {
            await _repositorioSolicitacao.AdicionarAsync(solicitacao);
            await _repositorioSolicitacao.SaveChangesAsync();
        }

        private async Task<SolicitacaoAntecipacao> ObterSolicitacaoPorId(Guid guidId)
        {
            var solicitacao = await _repositorioSolicitacao.ObterPorGuidIdAsync(guidId);
            if (solicitacao == null)
                throw new ArgumentException("Solicitação não encontrada");
            return solicitacao;
        }

        private SimulacaoDto CalcularSimulacao(decimal valor)
        {
            var valorLiquido = valor - (valor * TAXA_ANTECIPACAO);
            return new SimulacaoDto
            {
                ValorSolicitado = valor,
                TaxaAplicada = TAXA_ANTECIPACAO,
                ValorLiquido = valorLiquido
            };
        }

        private static MinhaSolicitacaoResponseDto MapearResponseDto(SolicitacaoAntecipacao solicitacao)
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
