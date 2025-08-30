using System;
using System.Collections.Concurrent;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using Poc.Micro.Ordering.Api.V1;
using Poc.Micro.Ordering.Domain.V1;

namespace Poc.Micro.Dispatcher.Api;

public record JobInfo(Guid JobId, JobState State, string Message);

public class JobStore
{
    private readonly Channel<Guid> _queue = Channel.CreateUnbounded<Guid>();
    private readonly ConcurrentDictionary<Guid, JobInfo> _kv = new();
    private readonly ConcurrentDictionary<Guid, Order> _orders = new();

    public ValueTask EnqueueAsync(Guid id) => _queue.Writer.WriteAsync(id);

    public IAsyncEnumerable<Guid> DequeueAllAsync(CancellationToken ct) => _queue.Reader.ReadAllAsync(ct);

    public void Upsert(JobInfo info) => _kv[info.JobId] = info;

    public JobInfo? Get(Guid id) => _kv.TryGetValue(id, out var v) ? v : null;

    public void SaveOrder(Guid id, Order order) => _orders[id] = order;

    public Order? GetOrder(Guid id) => _orders.TryGetValue(id, out var v) ? v : null;
}
