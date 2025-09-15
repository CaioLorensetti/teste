using Antecipacao.Domain.DTOs;
using FluentValidation;

namespace Antecipacao.Application.Validators
{
    public class CriarSolicitacaoValidator : AbstractValidator<CriarSolicitacaoDto>
    {
        public CriarSolicitacaoValidator()
        {
            RuleFor(x => x.CreatorId)
                .GreaterThan(0)
                .WithMessage("Creator ID deve ser maior que zero");

            RuleFor(x => x.ValorSolicitado)
                .GreaterThan(100)
                .WithMessage("Valor deve ser maior que R$ 100,00");

            RuleFor(x => x.DataSolicitacao)
                .LessThanOrEqualTo(DateTime.UtcNow)
                .WithMessage("Data de solicitação não pode ser futura");
        }
    }
}
