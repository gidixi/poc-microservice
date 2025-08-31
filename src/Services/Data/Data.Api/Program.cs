using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Poc.Micro.Data.Api;
using Poc.Micro.Data.Infrastructure;
using Poc.Micro.Ordering.Application.Orders;
using Poc.Micro.Ordering.Domain.Orders;
using Poc.Micro.Persistence.Abstractions;
using Poc.Micro.Persistence.EntityFramework;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
  options.ListenAnyIP(8080, listenOptions =>
  {
    listenOptions.Protocols = HttpProtocols.Http2; // IMPORTANTE!
  });
});

builder.Services.AddGrpc();
builder.Services.AddDbContext<OrderDbContext>(o => o.UseSqlite("Data Source=/data/orders.db"));
System.IO.Directory.CreateDirectory("/data");

// DbContext binding
builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<OrderDbContext>());

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
