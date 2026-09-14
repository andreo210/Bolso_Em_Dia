using BolsoEmDia.Front.Models.Request.Fatura;
using FluentValidation;

namespace BolsoEmDia.Front.Models.Validadores
{
    public class PagarFaturaRequestValidator : AbstractValidator<PagarFaturaRequest>
    {
        public PagarFaturaRequestValidator()
        {
            RuleFor(x => x.IdCategoria)
                .GreaterThan(0).WithMessage("Categoria é obrigatória");
        }
    }
}
