using System.Text.Json.Serialization;
using LancamentosService.Endpoints;
using LancamentosService.Persistence;
using LancamentosService.Workers;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddHealthChecks();
builder.Services.AddDbContext<LancamentosDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("LancamentosDb")));

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetValue<string>("Redis:ConnectionString") ?? "localhost:6379"));

builder.Services.AddHostedService<OutboxPublisherWorker>();

var app = builder.Build();

app.UseCors();
await app.MigrateAsync();

app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");
app.MapLancamentosEndpoints();

await app.RunAsync();

public partial class Program;
