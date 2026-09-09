using BolsoEmDia.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BolsoEmDia.Infra.Data.Configuracao
{
    public class RecorrenciaConfig : IEntityTypeConfiguration<Recorrencia>
    {
        public void Configure(EntityTypeBuilder<Recorrencia> builder)
        {
            builder.HasKey(e => e.IdRecorrencia);

            builder.Property(e => e.Valor).HasPrecision(18, 2);
            builder.Property(e => e.TipoTransacao).HasConversion<int>();
            builder.Property(e => e.Frequencia).HasConversion<int>();

            builder.HasOne<ApplicationUser>().WithMany()
                .HasForeignKey(e => e.IdUsuario)
                .OnDelete(DeleteBehavior.Cascade);

            // mutuamente exclusivos (validado em Recorrencia.Criar) — nenhum dos dois cascade,
            // o modelo não deve desaparecer sozinho enquanto a conta/cartão existir
            builder.HasOne<Conta>().WithMany()
                .HasForeignKey(e => e.IdConta)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Cartao>().WithMany()
                .HasForeignKey(e => e.IdCartao)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Categoria>().WithMany()
                .HasForeignKey(e => e.IdCategoria)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
