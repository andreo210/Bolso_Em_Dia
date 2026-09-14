using BolsoEmDia.Front.Models.Request.Orcamento;
using FluentValidation;

namespace BolsoEmDia.Front.Models.Validadores
{
    public class DefinirOrcamentoRequestValidator : AbstractValidator<DefinirOrcamentoRequest>
    {
        public DefinirOrcamentoRequestValidator()
        {
            RuleFor(x => x.IdCategoria)
                .GreaterThan(0).WithMessage("Categoria é obrigatória");

            RuleFor(x => x.MesReferencia)
                .NotEmpty().WithMessage("Mês de referência é obrigatório");

            RuleFor(x => x.ValorMeta)
                .GreaterThan(0).WithMessage("Valor da meta deve ser maior que zero");
        }
    }
}
