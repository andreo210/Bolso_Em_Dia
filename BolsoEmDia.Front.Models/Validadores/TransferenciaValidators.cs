using BolsoEmDia.Front.Models.Request.Transferencia;
using FluentValidation;

namespace BolsoEmDia.Front.Models.Validadores
{
    public class CriarTransferenciaRequestValidator : AbstractValidator<CriarTransferenciaRequest>
    {
        public CriarTransferenciaRequestValidator()
        {
            RuleFor(x => x.IdContaOrigem)
                .GreaterThan(0).WithMessage("Conta de origem é obrigatória");

            RuleFor(x => x.IdContaDestino)
                .GreaterThan(0).WithMessage("Conta de destino é obrigatória")
                .NotEqual(x => x.IdContaOrigem).WithMessage("Conta de origem e destino não podem ser a mesma");

            RuleFor(x => x.Data)
                .NotEmpty().WithMessage("Data é obrigatória");

            RuleFor(x => x.Valor)
                .GreaterThan(0).WithMessage("Valor deve ser maior que zero");

            RuleFor(x => x.Descricao)
                .MaximumLength(255).WithMessage("A descrição deve ter no máximo 255 caracteres");
        }
    }
}
