using BolsoEmDia.Front.Models.Request.Compra;
using FluentValidation;

namespace BolsoEmDia.Front.Models.Validadores
{
    public class CriarCompraRequestValidator : AbstractValidator<CriarCompraRequest>
    {
        public CriarCompraRequestValidator()
        {
            RuleFor(x => x.IdCartao)
                .GreaterThan(0).WithMessage("Cartão é obrigatório");

            RuleFor(x => x.IdCategoria)
                .GreaterThan(0).WithMessage("Categoria é obrigatória");

            RuleFor(x => x.Descricao)
                .NotEmpty().WithMessage("Descrição é obrigatória");

            RuleFor(x => x.ValorTotal)
                .GreaterThan(0).WithMessage("Valor total deve ser maior que zero");

            RuleFor(x => x.NumeroParcelas)
                .GreaterThanOrEqualTo(1).WithMessage("Número de parcelas deve ser maior ou igual a 1");
        }
    }
}
