using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Poc.Micro.Inventory.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcService<InventoryService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

app.Run();
