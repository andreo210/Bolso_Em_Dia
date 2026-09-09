using BolsoEmDia.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BolsoEmDia.Infra.Data.Configuracao
{
    public class MetaEconomiaConfig : IEntityTypeConfiguration<MetaEconomia>
    {
        public void Configure(EntityTypeBuilder<MetaEconomia> builder)
        {
            builder.HasKey(e => e.IdMeta);

            builder.Property(e => e.Nome).HasMaxLength(100).IsRequired();
            builder.Property(e => e.ValorAlvo).HasPrecision(18, 2);

            builder.HasOne<ApplicationUser>().WithMany()
                .HasForeignKey(e => e.IdUsuario)
                .OnDelete(DeleteBehavior.Cascade);

            // meta não precisa de conta dedicada
            builder.HasOne<Conta>().WithMany()
                .HasForeignKey(e => e.IdConta)
                .OnDelete(DeleteBehavior.SetNull);

            // aportes morrem com a meta
            builder.HasMany(e => e.Aportes).WithOne()
                .HasForeignKey(a => a.IdMeta)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(e => e.Aportes).UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
