using BolsoEmDia.Front.Models.Request.Meta;
using FluentValidation;

namespace BolsoEmDia.Front.Models.Validadores
{
    public class CriarMetaEconomiaRequestValidator : AbstractValidator<CriarMetaEconomiaRequest>
    {
        public CriarMetaEconomiaRequestValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Nome é obrigatório");

            RuleFor(x => x.ValorAlvo)
                .GreaterThan(0).WithMessage("Valor alvo deve ser maior que zero");
        }
    }

    public class RegistrarAporteMetaRequestValidator : AbstractValidator<RegistrarAporteMetaRequest>
    {
        public RegistrarAporteMetaRequestValidator()
        {
            RuleFor(x => x.Valor)
                .GreaterThan(0).WithMessage("Valor deve ser maior que zero");

            RuleFor(x => x.Data)
                .NotEmpty().WithMessage("Data é obrigatória");
        }
    }
}
