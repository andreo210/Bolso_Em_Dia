using BolsoEmDia.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BolsoEmDia.Infra.Data.Configuracao
{
    public class CartaoConfig : IEntityTypeConfiguration<Cartao>
    {
        public void Configure(EntityTypeBuilder<Cartao> builder)
        {
            builder.HasKey(e => e.IdCartao);

            builder.Property(e => e.Nome).HasMaxLength(100).IsRequired();
            builder.Property(e => e.LimiteTotal).HasPrecision(18, 2);

            //um usuario pode ter varios cartões, mas um cartão pertence a um usuario
            builder.HasOne<ApplicationUser>().WithMany()
                .HasForeignKey(e => e.IdUsuario)
                .OnDelete(DeleteBehavior.Cascade);// exclusão do usuário exclui todos os cartões dele

            //uma conta pode ter varios cartões, mas um cartão pertence a uma conta
            builder.HasOne<Conta>().WithMany()
                .HasForeignKey(e => e.IdContaPagamento)
                .OnDelete(DeleteBehavior.Restrict);

            // varias faturas pertencem a um cartão
            // faturas morrem com o cartão
            builder.HasMany(e => e.Faturas).WithOne()
                .HasForeignKey(f => f.IdCartao)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(e => e.Faturas).UsePropertyAccessMode(PropertyAccessMode.Field);// EF não precisa de setter público para coleção, mas precisa de acesso a campo privado
        }
    }
}
