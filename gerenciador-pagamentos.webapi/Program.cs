using gerenciador_pagamentos.webapi.Interface;
using gerenciador_pagamentos.webapi.Repository;
using gerenciador_pagamentos.webapi.Service;
using gerenciador_pagamentos.webapi.Infrastructure;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException(
        "Configure ConnectionStrings:Postgres por variável de ambiente, user-secrets ou appsettings.Development.json. Consulte o README.");

const string PoliticaCorsFront = "FrontLocal";

builder.Services.AddScoped<IJogadorService, JogadorService>();
builder.Services.AddSingleton(_ => NpgsqlDataSource.Create(connectionString));
builder.Services.AddScoped<IJogadorQueryRepository, JogadorQueryRepository>();
builder.Services.AddScoped<IJogadorCommandRepository, JogadorCommandRepository>();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddHealthChecks().AddCheck<PostgresHealthCheck>("postgres", tags: ["ready"]);

var origens = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy(PoliticaCorsFront, policy =>
        policy.WithOrigins(origens)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors(PoliticaCorsFront);

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health", new() { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new() { Predicate = check => check.Tags.Contains("ready") });

app.Run();

public partial class Program { }
