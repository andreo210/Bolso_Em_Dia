using BolsoEmDia.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BolsoEmDia.Infra.Data.Configuracao
{
    public class TransacaoConfig : IEntityTypeConfiguration<Transacao>
    {
        public void Configure(EntityTypeBuilder<Transacao> builder)
        {
            builder.HasKey(e => e.IdTransacao);

            builder.Property(e => e.Valor).HasPrecision(18, 2);
            builder.Property(e => e.Descricao).HasMaxLength(255);
            builder.Property(e => e.Tipo).HasConversion<int>();

            builder.HasOne<ApplicationUser>().WithMany()
                .HasForeignKey(e => e.IdUsuario)
                .OnDelete(DeleteBehavior.Cascade);

            // conta é obrigatória: bloqueia exclusão da conta enquanto houver histórico
            builder.HasOne<Conta>().WithMany()
                .HasForeignKey(e => e.IdConta)
                .OnDelete(DeleteBehavior.Restrict);

            // categoria é opcional (transferência não tem) — se a categoria some, o histórico fica sem categoria
            builder.HasOne<Categoria>().WithMany()
                .HasForeignKey(e => e.IdCategoria)
                .OnDelete(DeleteBehavior.SetNull);

            // apagar o modelo de recorrência não apaga nem desvincula as ocorrências já geradas
            builder.HasOne<Recorrencia>().WithMany()
                .HasForeignKey(e => e.IdRecorrencia)
                .OnDelete(DeleteBehavior.SetNull);

            // IdTransferencia é configurado do lado de Transferencia (TransferenciaConfig.Pernas)
        }
    }
}
