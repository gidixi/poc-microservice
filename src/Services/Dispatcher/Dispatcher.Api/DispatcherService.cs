using System;
using System.Threading.Tasks;
using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using Poc.Micro.Ordering.Api.V1;
using Poc.Micro.Ordering.Domain.V1;

namespace Poc.Micro.Dispatcher.Api;

using ApiDispatcher = Poc.Micro.Ordering.Api.V1.Dispatcher;

public class DispatcherService : ApiDispatcher.DispatcherBase
{
    private readonly JobStore _jobs;
    private readonly Logging.LoggingClient _log;
    private readonly Data.DataClient _data;

    public DispatcherService(JobStore jobs, Logging.LoggingClient log, Data.DataClient data)
    {
        _jobs = jobs;
        _log = log;
        _data = data;
    }

    public override async Task<SubmitOrderResponse> SubmitOrder(SubmitOrderRequest request, ServerCallContext context)
    {
        var jobId = CreateV7();
        _jobs.SaveOrder(jobId, request.Order);
        _jobs.Upsert(new JobInfo(jobId, JobState.Pending, "queued"));
        await _jobs.EnqueueAsync(jobId);

        await _log.WriteAsync(new LogEntry
        {
            Source = "dispatcher",
            Level = "INFO",
            CorrelationId = jobId.ToString(),
            Message = "Order received",
            UnixTsMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        });

        return new SubmitOrderResponse { JobId = new Uuid { Value = jobId.ToString() } };
    }

    public override async Task GetStatus(GetStatusRequest request, IServerStreamWriter<JobStatus> responseStream, ServerCallContext context)
    {
        var id = Guid.Parse(request.JobId.Value);
        while (!context.CancellationToken.IsCancellationRequested)
        {
            var j = _jobs.Get(id);
            if (j is not null)
            {
                await responseStream.WriteAsync(new JobStatus
                {
                    JobId = request.JobId,
                    State = j.State,
                    Message = j.Message
                });
                if (j.State is JobState.Persisted or JobState.Failed) break;
            }
            await Task.Delay(300, context.CancellationToken);
        }
    }

    public override Task<ListOrdersResponse> ListOrders(Empty request, ServerCallContext context)
        => _data.ListOrdersAsync(request, cancellationToken: context.CancellationToken).ResponseAsync;

    private static Guid CreateV7() => Guid.CreateVersion7(DateTimeOffset.UtcNow);
}
