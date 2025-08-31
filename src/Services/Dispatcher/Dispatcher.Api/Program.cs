using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Poc.Micro.Dispatcher.Api;
using Poc.Micro.Ordering.Api.V1;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddHealthChecks();
builder.Services.AddSingleton<JobStore>();
builder.Services.AddHostedService<DispatcherWorker>();

builder.Services.AddGrpcClient<Pricing.PricingClient>(o => o.Address = new Uri(GetEnv("PRICING_URL")));
builder.Services.AddGrpcClient<Inventory.InventoryClient>(o => o.Address = new Uri(GetEnv("INVENTORY_URL")));
builder.Services.AddGrpcClient<Data.DataClient>(o => o.Address = new Uri(GetEnv("DATA_URL")));
builder.Services.AddGrpcClient<Logging.LoggingClient>(o => o.Address = new Uri(GetEnv("LOG_URL")));

builder.Services.AddLogging();

builder.WebHost.ConfigureKestrel(options =>
{
  options.ListenAnyIP(8080, listenOptions =>
  {
    listenOptions.Protocols = HttpProtocols.Http2; // IMPORTANTE!
  });
});

var app = builder.Build();

using var scope = app.Services.CreateScope();
var logs = scope.ServiceProvider.GetRequiredService<Logging.LoggingClient>();
logs.WriteAsync(new LogEntry
{
  Source = "dispatcher",
  Level = "INFO",
  CorrelationId = "startup",
  Message = "service started",
  UnixTsMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
}).GetAwaiter().GetResult();

app.MapGrpcService<DispatcherService>();
app.MapGet("/healthz", () => Results.Ok("ok"));

app.Run();



static string GetEnv(string k) => Environment.GetEnvironmentVariable(k) ?? throw new InvalidOperationException($"Missing {k}");
