using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Grpc.Core;
using Poc.Micro.Ordering.Api.V1;

namespace Poc.Micro.Logger.Api;

public class LoggingService : Logging.LoggingBase
{
    private readonly LogStream _stream;

    public LoggingService(LogStream stream)
    {
        _stream = stream;
    }

    public override async Task<WriteAck> Write(LogEntry request, ServerCallContext context)
    {
        var line = $"{request.UnixTsMs} {request.Level} {request.Source}: {request.Message}";
        Console.WriteLine(line);
        await File.AppendAllTextAsync("/logs/app.log", line + Environment.NewLine);

        foreach (var writer in _stream.Get(request.CorrelationId))
        {
            await writer.WriteAsync(request);
        }

        return new WriteAck { Ok = true };
    }

    public override async Task Subscribe(LogFilter request, IServerStreamWriter<LogEntry> responseStream, ServerCallContext context)
    {
        _stream.Add(request.CorrelationId, responseStream);
        try
        {
            await Task.Delay(Timeout.Infinite, context.CancellationToken);
        }
        finally
        {
            _stream.Remove(request.CorrelationId, responseStream);
        }
    }
}

public class LogStream
{
    private readonly ConcurrentDictionary<string, List<IServerStreamWriter<LogEntry>>> _subs = new();

    public IEnumerable<IServerStreamWriter<LogEntry>> Get(string id)
        => _subs.TryGetValue(id, out var list) ? list : Array.Empty<IServerStreamWriter<LogEntry>>();

    public void Add(string id, IServerStreamWriter<LogEntry> writer)
    {
        var list = _subs.GetOrAdd(id, _ => new());
        lock (list) list.Add(writer);
    }

    public void Remove(string id, IServerStreamWriter<LogEntry> writer)
    {
        if (_subs.TryGetValue(id, out var list))
        {
            lock (list) list.Remove(writer);
            if (list.Count == 0) _subs.TryRemove(id, out _);
        }
    }
}
