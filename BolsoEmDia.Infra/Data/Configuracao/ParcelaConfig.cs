using BolsoEmDia.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BolsoEmDia.Infra.Data.Configuracao
{
    public class ParcelaConfig : IEntityTypeConfiguration<Parcela>
    {
        public void Configure(EntityTypeBuilder<Parcela> builder)
        {
            builder.HasKey(e => e.IdParcela);

            builder.Property(e => e.Valor).HasPrecision(18, 2);

            //um usuario pode ter varias parcelas, mas uma parcela pertence a um usuario
            builder.HasOne<ApplicationUser>().WithMany()
                .HasForeignKey(e => e.IdUsuario)
                .OnDelete(DeleteBehavior.Cascade);

            // consistente com Cartao → Fatura (Cascade): apagar o cartão precisa conseguir
            // apagar fatura e parcela em cascata sem esbarrar em Restrict no meio do caminho
            // uma fatura pode ter varias parcelas, mas uma parcela pertence a uma fatura
            builder.HasOne<Fatura>().WithMany()
                .HasForeignKey(e => e.IdFatura)
                .OnDelete(DeleteBehavior.Cascade);

            // IdCompra é configurado do lado de Compra (CompraConfig.Parcelas)
        }
    }
}
