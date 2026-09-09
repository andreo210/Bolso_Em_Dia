using BolsoEmDia.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BolsoEmDia.Infra.Data.Configuracao
{
    public class AporteMetaConfig : IEntityTypeConfiguration<AporteMeta>
    {
        public void Configure(EntityTypeBuilder<AporteMeta> builder)
        {
            builder.HasKey(e => e.IdAporte);

            builder.Property(e => e.Valor).HasPrecision(18, 2);

            builder.HasOne<ApplicationUser>().WithMany()
                .HasForeignKey(e => e.IdUsuario)
                .OnDelete(DeleteBehavior.Cascade);

            // transação/transferência que originou o aporte — opcional
            builder.HasOne<Transacao>().WithMany()
                .HasForeignKey(e => e.IdTransacao)
                .OnDelete(DeleteBehavior.SetNull);

            // IdMeta é configurado do lado de MetaEconomia (MetaEconomiaConfig.Aportes)
        }
    }
}
