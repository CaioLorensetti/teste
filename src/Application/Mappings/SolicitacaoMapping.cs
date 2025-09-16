using Antecipacao.Domain.DTOs;
using Antecipacao.Domain.Entities;

namespace Antecipacao.Application.Mappings
{
    public static class SolicitacaoMapping
    {
        public static MinhaSolicitacaoResponseDto ToResponseDto(this SolicitacaoAntecipacao solicitacao)
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
