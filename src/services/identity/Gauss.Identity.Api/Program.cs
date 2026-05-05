using Gauss.Identity.Api.Endpoints;
using Gauss.Identity.Api.HealthChecks;
using Gauss.Identity.Api.Installers;
using Gauss.Identity.Api.Observability;
using Gauss.Identity.Api.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.InstallServices(
    builder.Configuration,
    typeof(Program).Assembly);

var app = builder.Build();

app.UseGaussCorrelationId();

app.MapGaussOpenApi();

app.MapGaussHealthChecks();

app.UseAuthentication();

app.UseAuthorization();

app.MapIdentityEndpoints();

await app.RunAsync();

public partial class Program;
