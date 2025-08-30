using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Poc.Micro.Data.Api;
using Poc.Micro.Data.Infrastructure;
using Poc.Micro.Ordering.Application.Orders;
using Poc.Micro.Ordering.Domain.Orders;
using Poc.Micro.Persistence.Abstractions;
using Poc.Micro.Persistence.EntityFramework;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddDbContext<OrderDbContext>(o => o.UseSqlite("Data Source=/data/orders.db"));
System.IO.Directory.CreateDirectory("/data");

// Domain/Application bindings
builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IOrdersAppService, OrdersAppService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    db.Database.EnsureCreated();
}

app.MapGrpcService<DataService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

app.Run();
