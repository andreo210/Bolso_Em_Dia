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
        }
    }
}
