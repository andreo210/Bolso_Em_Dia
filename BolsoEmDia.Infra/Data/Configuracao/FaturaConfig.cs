using BolsoEmDia.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BolsoEmDia.Infra.Data.Configuracao
{
    public class FaturaConfig : IEntityTypeConfiguration<Fatura>
    {
        public void Configure(EntityTypeBuilder<Fatura> builder)
        {
            builder.HasKey(e => e.IdFatura);

            builder.Property(e => e.ValorTotal).HasPrecision(18, 2);
            builder.Property(e => e.Status).HasConversion<int>();

            builder.HasOne<ApplicationUser>().WithMany()
                .HasForeignKey(e => e.IdUsuario)
                .OnDelete(DeleteBehavior.Cascade);

            // preenchido só quando Status = Paga
            builder.HasOne<Transacao>().WithMany()
                .HasForeignKey(e => e.IdTransacaoPagamento)
                .OnDelete(DeleteBehavior.SetNull);

            // uma fatura por cartão por mês
            builder.HasIndex(e => new { e.IdCartao, e.MesReferencia }).IsUnique();

            // IdCartao é configurado do lado de Cartao (CartaoConfig.Faturas)
        }
    }
}
