using System.Reflection;
using BolsoEmDia.Domain;
using BolsoEmDia.Domain.Entidades;
using BolsoEmDia.Infra.Data.CurrentUsers;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BolsoEmDia.Infra.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly string? _currentUserId;

        public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUser currentUser)
            : base(options)
        {
            _currentUserId = currentUser.UserId;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // primeiro, para não perder o mapeamento do Identity

            builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            builder.AplicarConversorDataUtc();
            AplicarFiltroPorUsuario(builder);
        }

        private void AplicarFiltroPorUsuario(ModelBuilder builder)
        {
            var metodo = typeof(AppDbContext).GetMethod(nameof(DefinirFiltroDoUsuario), BindingFlags.NonPublic | BindingFlags.Instance)!;

            foreach (var entidade in builder.Model.GetEntityTypes())
            {
                if (typeof(IPertenceAoUsuario).IsAssignableFrom(entidade.ClrType))
                    metodo.MakeGenericMethod(entidade.ClrType).Invoke(this, new object[] { builder });
            }
        }

        private void DefinirFiltroDoUsuario<TEntity>(ModelBuilder builder) where TEntity : class, IPertenceAoUsuario
        {
            builder.Entity<TEntity>().HasQueryFilter(e => e.IdUsuario == _currentUserId);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            var usuario = _currentUserId ?? "SYSTEM";
            ChangeTracker.AplicarAuditoria(usuario);
            this.CriarHistoricoTemporal(usuario);
            return await base.SaveChangesAsync(ct);
        }
    }
}
