using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Poc.Micro.Logger.Api;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
  options.ListenAnyIP(8080, listenOptions =>
  {
    listenOptions.Protocols = HttpProtocols.Http2; // IMPORTANTE!
  });
});

builder.Services.AddSingleton<LogStream>();
builder.Services.AddGrpc();
System.IO.Directory.CreateDirectory("/logs");

var app = builder.Build();

app.MapGrpcService<LoggingService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

app.Run();
