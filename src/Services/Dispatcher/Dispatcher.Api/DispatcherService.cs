using Grpc.Core;
using Poc.Micro.Ordering.Api.V1;
using Poc.Micro.Ordering.Domain.V1;
using System;
using System.Threading.Tasks;

namespace Poc.Micro.Dispatcher.Api;

using ApiDispatcher = Poc.Micro.Ordering.Api.V1.Dispatcher;

public class DispatcherService : ApiDispatcher.DispatcherBase
{
    public override Task<SubmitOrderResponse> SubmitOrder(SubmitOrderRequest request, ServerCallContext context)
    {
        var jobId = new Uuid { Value = Guid.NewGuid().ToString() };
        return Task.FromResult(new SubmitOrderResponse { JobId = jobId });
    }

    public override async Task GetStatus(GetStatusRequest request, IServerStreamWriter<JobStatus> responseStream, ServerCallContext context)
    {
        await responseStream.WriteAsync(new JobStatus
        {
            JobId = request.JobId,
            State = JobState.Pending,
            Message = "Pending"
        });
    }
}
