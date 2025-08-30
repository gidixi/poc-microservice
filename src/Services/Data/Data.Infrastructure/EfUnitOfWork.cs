using Microsoft.EntityFrameworkCore;
using Poc.Micro.Ordering.Domain.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Poc.Micro.Data.Infrastructure;

public class EfUnitOfWork : IUnitOfWork
{
    private readonly OrderDbContext _db;
    public EfUnitOfWork(OrderDbContext db) => _db = db;

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);

    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken ct = default)
    {
        var strategy = _db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);
            await action(ct);
            await tx.CommitAsync(ct);
        });
    }
}
