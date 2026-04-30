using Gauss.Identity.Api.HealthChecks;
using Gauss.Identity.Api.Installers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.InstallServices(
    builder.Configuration,
    typeof(Program).Assembly);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGaussHealthChecks();

await app.RunAsync();
