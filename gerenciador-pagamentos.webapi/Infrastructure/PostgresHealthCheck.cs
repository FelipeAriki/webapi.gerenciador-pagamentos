using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace gerenciador_pagamentos.webapi.Infrastructure;

public class PostgresHealthCheck(NpgsqlDataSource dataSource) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            await using var command = new NpgsqlCommand("SELECT 1 FROM jogador LIMIT 1", connection) { CommandTimeout = 5 };
            await command.ExecuteScalarAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return HealthCheckResult.Unhealthy("Banco ou tabela de jogadores indisponível.");
        }
    }
}
