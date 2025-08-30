using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using Poc.Micro.Dispatcher.Api;
using Poc.Micro.Ordering.Api.V1;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddHealthChecks();
builder.Services.AddSingleton<JobStore>();
builder.Services.AddHostedService<DispatcherWorker>();

builder.Services.AddGrpcClient<Pricing.PricingClient>(o => o.Address = new Uri(GetEnv("PRICING_URL")));
builder.Services.AddGrpcClient<Inventory.InventoryClient>(o => o.Address = new Uri(GetEnv("INVENTORY_URL")));
builder.Services.AddGrpcClient<Data.DataClient>(o => o.Address = new Uri(GetEnv("DATA_URL")));
builder.Services.AddGrpcClient<Logging.LoggingClient>(o => o.Address = new Uri(GetEnv("LOG_URL")));

var app = builder.Build();

app.MapGrpcService<DispatcherService>();
app.MapGet("/healthz", () => Results.Ok("ok"));

app.Run();

static string GetEnv(string k) => Environment.GetEnvironmentVariable(k) ?? throw new InvalidOperationException($"Missing {k}");
