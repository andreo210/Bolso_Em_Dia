using BolsoEmDia.Domain.IRepositorio;

namespace BolsoEmDia.Tests.Fakes
{
    /// <summary>
    /// <see cref="IUnitOfWork"/> em memória: os fakes de repositório já gravam na hora (ver
    /// <see cref="RepositorioFake{TEntity}"/>), então aqui não há SaveChanges/transação de banco de
    /// verdade para simular — só o controle de "abriu/commitou", para o serviço poder afirmar que
    /// passou pelo fluxo transacional em vez de gravar as pernas soltas.
    /// </summary>
    public class UnitOfWorkFake : IUnitOfWork
    {
        public bool HasActiveTransaction { get; private set; }

        public int Commits { get; private set; }

        public int Rollbacks { get; private set; }

        public Task BeginTransactionAsync(CancellationToken ct = default)
        {
            HasActiveTransaction = true;
            return Task.CompletedTask;
        }

        public Task CommitAsync(CancellationToken ct = default)
        {
            HasActiveTransaction = false;
            Commits++;
            return Task.CompletedTask;
        }

        public Task RollbackAsync(CancellationToken ct = default)
        {
            HasActiveTransaction = false;
            Rollbacks++;
            return Task.CompletedTask;
        }

        public async Task<T> ExecuteTransactionAsync<T>(Func<Task<T>> action, CancellationToken ct = default)
        {
            await BeginTransactionAsync(ct);

            try
            {
                var resultado = await action();
                await CommitAsync(ct);
                return resultado;
            }
            catch
            {
                await RollbackAsync(ct);
                throw;
            }
        }

        public void Dispose() { }
    }
}
