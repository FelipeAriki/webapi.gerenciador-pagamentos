using gerenciador_pagamentos.webapi.Interface;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace gerenciador_pagamentos.webapi.Tests;

public class ApiFactory : WebApplicationFactory<Program>
{
    public MemoryJogadorRepository Repository { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureLogging(logging => logging.ClearProviders().AddConsole());
        builder.UseSetting("ConnectionStrings:Postgres", "Host=localhost;Database=testes_sem_conexao");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IJogadorQueryRepository>();
            services.RemoveAll<IJogadorCommandRepository>();
            services.AddSingleton<IJogadorQueryRepository>(Repository);
            services.AddSingleton<IJogadorCommandRepository>(Repository);
        });
    }
}
