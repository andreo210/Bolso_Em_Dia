using BolsoEmDia.Front.Models.Enum;
using BolsoEmDia.Front.Models.Request.Conta;
using FluentValidation;

namespace BolsoEmDia.Front.Models.Validadores
{
    public class CriarContaRequestValidator : AbstractValidator<CriarContaRequest>
    {
        public CriarContaRequestValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("O nome da conta é obrigatório")
                .MaximumLength(100).WithMessage("O nome deve ter no máximo 100 caracteres");

            RuleFor(x => x.Tipo)
                .IsInEnum().WithMessage("Tipo de conta inválido");

            // Espelha Conta.Criar no domínio: só conta corrente aceita cheque especial.
            RuleFor(x => x.SaldoInicial)
                .Must((request, saldoInicial) => saldoInicial >= 0 || request.Tipo == TipoConta.Corrente)
                .WithMessage("Somente conta corrente pode ter saldo inicial negativo");
        }
    }

    public class AtualizarContaRequestValidator : AbstractValidator<AtualizarContaRequest>
    {
        public AtualizarContaRequestValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("O nome da conta é obrigatório")
                .MaximumLength(100).WithMessage("O nome deve ter no máximo 100 caracteres");
        }
    }
}
