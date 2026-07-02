using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ConsolidadoService.Persistence;

public static class DatabaseBootstrapper
{
    public static async Task EnsureDatabaseExistsAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var databaseName = builder.Database;
        builder.Database = "postgres";

        await using var connection = new NpgsqlConnection(builder.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var existsCommand = connection.CreateCommand();
        existsCommand.CommandText = "select 1 from pg_database where datname = @database";
        existsCommand.Parameters.AddWithValue("database", databaseName ?? string.Empty);
        var exists = await existsCommand.ExecuteScalarAsync(cancellationToken) is not null;

        if (exists)
        {
            return;
        }

        await using var createCommand = connection.CreateCommand();
        createCommand.CommandText = $"create database \"{databaseName}\"";
        await createCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    public static async Task MigrateAsync(this WebApplication app)
    {
        var connectionString = app.Configuration.GetConnectionString("ConsolidadoDb")
            ?? throw new InvalidOperationException("ConnectionStrings:ConsolidadoDb não configurada.");

        await EnsureDatabaseExistsAsync(connectionString);

        await using var scope = app.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ConsolidadoDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}
