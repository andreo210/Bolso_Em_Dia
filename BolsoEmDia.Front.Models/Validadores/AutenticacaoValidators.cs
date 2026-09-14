using BolsoEmDia.Front.Models.Request.Autenticacao;
using FluentValidation;

namespace BolsoEmDia.Front.Models.Validadores
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("O e-mail é obrigatório")
                .EmailAddress().WithMessage("E-mail inválido");

            RuleFor(x => x.Senha)
                .NotEmpty().WithMessage("A senha é obrigatória");
        }
    }

    public class RegistrarRequestValidator : AbstractValidator<RegistrarRequest>
    {
        public RegistrarRequestValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("O nome é obrigatório")
                .MaximumLength(100).WithMessage("O nome deve ter no máximo 100 caracteres");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("O e-mail é obrigatório")
                .EmailAddress().WithMessage("E-mail inválido");

            RuleFor(x => x.Senha)
                .NotEmpty().WithMessage("A senha é obrigatória");
        }
    }
}
