using BolsoEmDia.Front.Models.Request.Categoria;
using FluentValidation;

namespace BolsoEmDia.Front.Models.Validadores
{
    public class CriarCategoriaRequestValidator : AbstractValidator<CriarCategoriaRequest>
    {
        public CriarCategoriaRequestValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Nome é obrigatório")
                .MaximumLength(100).WithMessage("O nome deve ter no máximo 100 caracteres");

            RuleFor(x => x.Tipo)
                .IsInEnum().WithMessage("Tipo é obrigatório");

            RuleFor(x => x.Cor)
                .NotEmpty().WithMessage("Cor é obrigatória")
                .Matches("^#[0-9A-Fa-f]{6}$").WithMessage("Cor deve estar no formato hexadecimal (#rrggbb)");

            RuleFor(x => x.Icone)
                .NotEmpty().WithMessage("Ícone é obrigatório")
                .MaximumLength(50).WithMessage("O ícone deve ter no máximo 50 caracteres");
        }
    }

    public class AtualizarCategoriaRequestValidator : AbstractValidator<AtualizarCategoriaRequest>
    {
        public AtualizarCategoriaRequestValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Nome é obrigatório")
                .MaximumLength(100).WithMessage("O nome deve ter no máximo 100 caracteres");

            RuleFor(x => x.Cor)
                .NotEmpty().WithMessage("Cor é obrigatória")
                .Matches("^#[0-9A-Fa-f]{6}$").WithMessage("Cor deve estar no formato hexadecimal (#rrggbb)");

            RuleFor(x => x.Icone)
                .NotEmpty().WithMessage("Ícone é obrigatório")
                .MaximumLength(50).WithMessage("O ícone deve ter no máximo 50 caracteres");
        }
    }
}
