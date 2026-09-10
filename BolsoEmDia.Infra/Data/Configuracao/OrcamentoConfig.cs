using BolsoEmDia.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BolsoEmDia.Infra.Data.Configuracao
{
    public class OrcamentoConfig : IEntityTypeConfiguration<Orcamento>
    {
        public void Configure(EntityTypeBuilder<Orcamento> builder)
        {
            builder.HasKey(e => e.IdOrcamento);

            builder.Property(e => e.ValorMeta).HasPrecision(18, 2);

            builder.HasOne<ApplicationUser>().WithMany()
                .HasForeignKey(e => e.IdUsuario)
                .OnDelete(DeleteBehavior.Cascade);


            //uma categoria tem muitos orçamentos, mas um orçamento pertence a uma categoria
            builder.HasOne<Categoria>().WithMany()
                .HasForeignKey(e => e.IdCategoria)
                .OnDelete(DeleteBehavior.Cascade);

            // um orçamento por categoria por mês — inserir duplicata é erro de aplicação
            builder.HasIndex(e => new { e.IdCategoria, e.MesReferencia }).IsUnique();
        }
    }
}
