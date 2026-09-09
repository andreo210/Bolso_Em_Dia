using BolsoEmDia.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BolsoEmDia.Infra.Data.Configuracao
{
    public class CategoriaConfig : IEntityTypeConfiguration<Categoria>
    {
        public void Configure(EntityTypeBuilder<Categoria> builder)
        {
            builder.HasKey(e => e.IdCategoria);

            builder.Property(e => e.Nome).HasMaxLength(100).IsRequired();
            builder.Property(e => e.Tipo).HasConversion<int>();

            //uma categoria pertence a varios usuarios, mas um usuario pode ter varias categorias
            builder.HasOne<ApplicationUser>().WithMany()
                .HasForeignKey(e => e.IdUsuario)
                .OnDelete(DeleteBehavior.Cascade);// exclusão do usuário exclui todas as categorias dele


            // hierarquia de um nível só (validada em Categoria.Criar); exclusão da mãe é bloqueada
            // enquanto houver subcategoria
            // uma categoria pode ter uma categoria pai, mas uma categoria pai pode ter varias categorias filhas
            builder.HasOne<Categoria>().WithMany()
                .HasForeignKey(e => e.IdCategoriaPai)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
