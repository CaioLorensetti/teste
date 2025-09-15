using Antecipacao.Domain.DTOs;
using Antecipacao.Domain.Entities;

namespace Antecipacao.Application.Mappings
{
    public static class SolicitacaoMapping
    {
        public static SolicitacaoResponseDto ToResponseDto(this SolicitacaoAntecipacao solicitacao)
        {
            return new SolicitacaoResponseDto
            {
                GuidId = solicitacao.GuidId,
                CreatorId = solicitacao.CreatorId,
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
