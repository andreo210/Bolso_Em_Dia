using BolsoEmDia.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BolsoEmDia.Infra.Data.Configuracao
{
    public class CompraConfig : IEntityTypeConfiguration<Compra>
    {
        public void Configure(EntityTypeBuilder<Compra> builder)
        {
            builder.HasKey(e => e.IdCompra);

            builder.Property(e => e.Descricao).HasMaxLength(255).IsRequired();
            builder.Property(e => e.ValorTotal).HasPrecision(18, 2);

            //um usuario pode ter varias compras, mas uma compra pertence a um usuario
            builder.HasOne<ApplicationUser>().WithMany()
                .HasForeignKey(e => e.IdUsuario)
                .OnDelete(DeleteBehavior.Cascade);// exclusão do usuário exclui todas as compras dele

            //um cartão pode ter varias compras, mas uma compra pertence a um cartão
            builder.HasOne<Cartao>().WithMany()
                .HasForeignKey(e => e.IdCartao)
                .OnDelete(DeleteBehavior.Restrict);// exclusão do cartão não exclui as compras dele

            //uma categoria pode ter varias compras, mas uma compra pertence a uma categoria
            builder.HasOne<Categoria>().WithMany()
                .HasForeignKey(e => e.IdCategoria)
                .OnDelete(DeleteBehavior.Restrict);// exclusão da categoria não exclui as compras dela

            // apagar o modelo de recorrência não apaga as compras já geradas
            //uma recorrência pode ter varias compras, mas uma compra pertence a uma recorrência
            builder.HasOne<Recorrencia>().WithMany()
                .HasForeignKey(e => e.IdRecorrencia)
                .OnDelete(DeleteBehavior.SetNull);// exclusão da recorrência não exclui as compras dela

            // parcelas morrem com a compra
            //varias parcelas pertencem a uma compra, mas uma compra pode ter varias parcelas
            builder.HasMany(e => e.Parcelas).WithOne()
                .HasForeignKey(p => p.IdCompra)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(e => e.Parcelas).UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
