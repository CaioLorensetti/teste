using Antecipacao.Domain.DTOs;
using FluentValidation;

namespace Antecipacao.Application.Validators
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty()
                .WithMessage("Username é obrigatório")
                .EmailAddress()
                .WithMessage("Username deve ser um email válido");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Senha é obrigatória");
        }
    }
}
