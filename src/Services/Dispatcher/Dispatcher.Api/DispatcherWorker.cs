using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Poc.Micro.Ordering.Api.V1;

namespace Poc.Micro.Dispatcher.Api;

public class DispatcherWorker : BackgroundService
{
    private readonly JobStore _jobs;
    private readonly Pricing.PricingClient _pricing;
    private readonly Inventory.InventoryClient _inventory;
    private readonly Data.DataClient _data;
    private readonly Logging.LoggingClient _log;

    public DispatcherWorker(JobStore jobs, Pricing.PricingClient pricing, Inventory.InventoryClient inventory, Data.DataClient data, Logging.LoggingClient log)
    {
        _jobs = jobs;
        _pricing = pricing;
        _inventory = inventory;
        _data = data;
        _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var jobId in _jobs.DequeueAllAsync(stoppingToken))
        {
            try
            {
                _jobs.Upsert(new(jobId, JobState.Pending, "pricing"));
                var order = _jobs.GetOrder(jobId) ?? throw new InvalidOperationException("Order not found");
                var priced = await _pricing.CalculateAsync(order, cancellationToken: stoppingToken);

                _jobs.Upsert(new(jobId, JobState.Priced, "inventory"));
                var res = await _inventory.ReserveAsync(priced.Order, cancellationToken: stoppingToken);
                if (!res.Reserved) throw new InvalidOperationException(res.Reason);

                _jobs.Upsert(new(jobId, JobState.Reserved, "persisting"));
                var saved = await _data.SaveOrderAsync(priced, cancellationToken: stoppingToken);
                if (string.IsNullOrEmpty(saved.Value)) throw new InvalidOperationException("save failed");

                _jobs.Upsert(new(jobId, JobState.Persisted, "done"));
            }
            catch (Exception ex)
            {
                _jobs.Upsert(new(jobId, JobState.Failed, ex.Message));
                await _log.WriteAsync(new LogEntry
                {
                    Source = "dispatcher",
                    Level = "ERROR",
                    CorrelationId = jobId.ToString(),
                    Message = ex.Message,
                    UnixTsMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                }, cancellationToken: stoppingToken);
            }
        }
    }
}
