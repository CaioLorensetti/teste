using Antecipacao.Domain.DTOs;
using FluentValidation;

namespace Antecipacao.Application.Validators
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.Fullname)
                .NotEmpty().WithMessage("Nome completo é obrigatório");

            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Usuário é obrigatório")
                .EmailAddress().WithMessage("Usuário deve ser um email válido");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Senha é obrigatória")
                .MinimumLength(8).WithMessage("Senha deve ter pelo menos 8 caracteres")
                .Matches("[A-Z]").WithMessage("Senha deve conter ao menos uma letra maiúscula")
                .Matches("[a-z]").WithMessage("Senha deve conter ao menos uma letra minúscula")
                .Matches("[0-9]").WithMessage("Senha deve conter ao menos um número")
                .Matches(@"[!@#$%^&*()_+\-=\[\]{};':""|,.<>/?]").WithMessage("Senha deve conter ao menos um caractere especial");
        }
    }
}
