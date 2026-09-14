using BolsoEmDia.Front.Models.Request.Transacao;
using FluentValidation;

namespace BolsoEmDia.Front.Models.Validadores
{
    public class CriarReceitaRequestValidator : AbstractValidator<CriarReceitaRequest>
    {
        public CriarReceitaRequestValidator()
        {
            RuleFor(x => x.IdConta)
                .GreaterThan(0).WithMessage("Conta é obrigatória");

            RuleFor(x => x.IdCategoria)
                .GreaterThan(0).WithMessage("Categoria é obrigatória");

            RuleFor(x => x.Data)
                .NotEmpty().WithMessage("Data é obrigatória");

            RuleFor(x => x.Valor)
                .GreaterThan(0).WithMessage("Valor deve ser maior que zero");

            RuleFor(x => x.Descricao)
                .MaximumLength(255).WithMessage("A descrição deve ter no máximo 255 caracteres");
        }
    }

    public class CriarDespesaRequestValidator : AbstractValidator<CriarDespesaRequest>
    {
        public CriarDespesaRequestValidator()
        {
            RuleFor(x => x.IdConta)
                .GreaterThan(0).WithMessage("Conta é obrigatória");

            RuleFor(x => x.IdCategoria)
                .GreaterThan(0).WithMessage("Categoria é obrigatória");

            RuleFor(x => x.Data)
                .NotEmpty().WithMessage("Data é obrigatória");

            RuleFor(x => x.Valor)
                .GreaterThan(0).WithMessage("Valor deve ser maior que zero");

            RuleFor(x => x.Descricao)
                .MaximumLength(255).WithMessage("A descrição deve ter no máximo 255 caracteres");
        }
    }
}
