using BolsoEmDia.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BolsoEmDia.Infra.Data.Configuracao
{
    public class TransferenciaConfig : IEntityTypeConfiguration<Transferencia>
    {
        public void Configure(EntityTypeBuilder<Transferencia> builder)
        {
            builder.HasKey(e => e.IdTransferencia);

            builder.Property(e => e.Valor).HasPrecision(18, 2);
            builder.Property(e => e.Descricao).HasMaxLength(255);

            // um usuario pode ter varias transferencias, mas uma transferencia pertence a um usuario
            builder.HasOne<ApplicationUser>().WithMany()
                .HasForeignKey(e => e.IdUsuario)
                .OnDelete(DeleteBehavior.Cascade);// exclusão do usuário exclui todas as transferencias dele

            // uma conta pode ter varias transferencias de origem, mas uma transferencia de origem pertence a uma conta
            builder.HasOne<Conta>().WithMany()
                .HasForeignKey(e => e.IdContaOrigem)
                .OnDelete(DeleteBehavior.Restrict);// exclusão da conta não exclui as transferencias de origem dela

            // uma conta pode ter varias transferencias de destino, mas uma transferencia de destino pertence a uma conta
            builder.HasOne<Conta>().WithMany()
                .HasForeignKey(e => e.IdContaDestino)
                .OnDelete(DeleteBehavior.Restrict);// exclusão da conta não exclui as transferencias de destino dela

            // as duas pernas nascem e morrem com a Transferencia — nunca fica uma órfã
            builder.HasMany(e => e.Pernas).WithOne()
                .HasForeignKey(p => p.IdTransferencia)
                .OnDelete(DeleteBehavior.Cascade);// exclusão da transferencia exclui todas as pernas dela

            // Pernas só tem getter (backing field _pernas) — EF precisa escrever direto no campo
            builder.Navigation(e => e.Pernas).UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
