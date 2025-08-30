using Microsoft.EntityFrameworkCore;
using Poc.Micro.Ordering.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Poc.Micro.Data.Infrastructure;

public class Repository<T> : IRepository<T> where T : class, IAggregateRoot
{
    protected readonly OrderDbContext _db;
    protected readonly DbSet<T> _set;

    public Repository(OrderDbContext db)
    {
        _db = db;
        _set = db.Set<T>();
    }

    public Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _set.FindAsync(new object?[] { id }, ct).AsTask();

    public Task AddAsync(T entity, CancellationToken ct = default)
        => _set.AddAsync(entity, ct).AsTask();

    public void Update(T entity) => _set.Update(entity);
    public void Remove(T entity) => _set.Remove(entity);

    public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => _set.AnyAsync(predicate, ct);

    public async Task<List<T>> ListAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default)
        => predicate is null ? await _set.ToListAsync(ct) : await _set.Where(predicate).ToListAsync(ct);
}
