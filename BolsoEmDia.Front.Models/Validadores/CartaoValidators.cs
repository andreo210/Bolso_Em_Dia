using BolsoEmDia.Front.Models.Request.Cartao;
using FluentValidation;

namespace BolsoEmDia.Front.Models.Validadores
{
    public class CriarCartaoRequestValidator : AbstractValidator<CriarCartaoRequest>
    {
        public CriarCartaoRequestValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Nome é obrigatório");

            RuleFor(x => x.LimiteTotal)
                .GreaterThan(0).WithMessage("Limite total deve ser maior que zero");

            RuleFor(x => x.DiaFechamento)
                .InclusiveBetween(1, 31).WithMessage("Dia de fechamento deve estar entre 1 e 31");

            RuleFor(x => x.DiaVencimento)
                .InclusiveBetween(1, 31).WithMessage("Dia de vencimento deve estar entre 1 e 31");

            RuleFor(x => x.IdContaPagamento)
                .GreaterThan(0).WithMessage("Conta de pagamento é obrigatória");
        }
    }
}
