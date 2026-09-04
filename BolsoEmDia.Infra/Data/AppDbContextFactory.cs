using BolsoEmDia.Infra.Data.CurrentUsers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BolsoEmDia.Infra.Data
{
    /// <summary>
    /// Sem isso, "dotnet ef" quebra ao resolver o contexto: AppDbContext depende de
    /// ICurrentUser, que depende de HttpContext, que não existe em tempo de design.
    /// </summary>
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__BolsoEmDia")
                ?? "Host=localhost;Port=5432;Database=bolso_em_dia;Username=admin;Password=admin123";

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention()
                .Options;

            return new AppDbContext(options, new CurrentUserDesignTime());
        }

        private class CurrentUserDesignTime : ICurrentUser
        {
            public string? UserId => null;
            public bool IsAuthenticated => false;
        }
    }
}
