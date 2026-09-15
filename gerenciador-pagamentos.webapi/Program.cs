using gerenciador_pagamentos.webapi.Interface;
using gerenciador_pagamentos.webapi.Repository;
using gerenciador_pagamentos.webapi.Service;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException(
        "Connection string 'Postgres' nao configurada. Verifique appsettings.Development.json.");

const string PoliticaCorsFront = "FrontLocal";

builder.Services.AddScoped<IJogadorService, JogadorService>();
builder.Services.AddScoped<IJogadorQueryRepository>(_ => new JogadorQueryRepository(connectionString));
builder.Services.AddScoped<IJogadorCommandRepository>(_ => new JogadorCommandRepository(connectionString));

builder.Services.AddCors(options =>
{
    options.AddPolicy(PoliticaCorsFront, policy =>
        policy.WithOrigins(
                  "http://localhost:5173",
                  "http://127.0.0.1:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

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

app.Run();
