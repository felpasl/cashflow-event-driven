using ConsolidadoService.Endpoints;
using ConsolidadoService.Infrastructure;
using ConsolidadoService.Persistence;
using ConsolidadoService.Workers;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddHealthChecks();
builder.Services.AddDbContext<ConsolidadoDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ConsolidadoDb")));

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetValue<string>("Redis:ConnectionString") ?? "localhost:6379"));
builder.Services.AddScoped<ConsolidadoCache>();
builder.Services.AddHostedService<LancamentosStreamConsumerWorker>();

var app = builder.Build();

app.UseCors();
await app.MigrateAsync();

app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");
app.MapConsolidadosEndpoints();

await app.RunAsync();

public partial class Program;
