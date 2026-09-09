using BolsoEmDia.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BolsoEmDia.Infra.Data.Configuracao
{
    public class ContaConfig : IEntityTypeConfiguration<Conta>
    {
        public void Configure(EntityTypeBuilder<Conta> builder)
        {
            builder.HasKey(e => e.IdConta);

            builder.Property(e => e.Nome).HasMaxLength(100).IsRequired();
            builder.Property(e => e.SaldoInicial).HasPrecision(18, 2);
            builder.Property(e => e.Tipo).HasConversion<int>();

            //um usuario pode ter varias contas, mas uma conta pertence a um usuario
            builder.HasOne<ApplicationUser>().WithMany()
                .HasForeignKey(e => e.IdUsuario)
                .OnDelete(DeleteBehavior.Cascade);// exclusão do usuário exclui todas as contas dele
        }
    }
}
