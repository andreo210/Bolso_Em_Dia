using BolsoEmDia.Front.Models.Enum;
using BolsoEmDia.Front.Models.Request.Recorrencia;
using FluentValidation;

namespace BolsoEmDia.Front.Models.Validadores
{
    public class CriarRecorrenciaRequestValidator : AbstractValidator<CriarRecorrenciaRequest>
    {
        public CriarRecorrenciaRequestValidator()
        {
            // Espelha Recorrencia.Criar no domínio: exatamente uma entre conta e cartão.
            RuleFor(x => x)
                .Must(x => (x.IdConta is > 0) != (x.IdCartao is > 0))
                .WithMessage("Selecione uma conta ou um cartão, nunca os dois");

            RuleFor(x => x.TipoTransacao)
                .NotNull().WithMessage("Tipo da transação é obrigatório quando a recorrência gera lançamento em conta")
                .When(x => x.IdConta is > 0);

            RuleFor(x => x.IdCategoria)
                .GreaterThan(0).WithMessage("Categoria é obrigatória");

            RuleFor(x => x.Valor)
                .GreaterThan(0).WithMessage("Valor deve ser maior que zero");

            RuleFor(x => x.Frequencia)
                .IsInEnum().WithMessage("Frequência é obrigatória");

            RuleFor(x => x.DiaGeracao)
                .InclusiveBetween(0, 6).WithMessage("Dia da semana inválido")
                .When(x => x.Frequencia == FrequenciaRecorrencia.Semanal);

            RuleFor(x => x.DiaGeracao)
                .InclusiveBetween(1, 31).WithMessage("Dia do mês inválido")
                .When(x => x.Frequencia != FrequenciaRecorrencia.Semanal);

            RuleFor(x => x.DataFim)
                .Must((request, dataFim) => !dataFim.HasValue || dataFim.Value.Date >= request.DataInicio.Date)
                .WithMessage("Data fim não pode ser anterior à data de início");
        }
    }
}
