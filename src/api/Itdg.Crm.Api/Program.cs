using Azure.Monitor.OpenTelemetry.AspNetCore;
using FluentValidation;
using Itdg.Crm.Api.Extensions;
using Itdg.Crm.Api.Middlewares;
using Itdg.Crm.Api.Infrastructure.Extensions;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Infrastructure (DbContext, Repositories)
builder.Services.AddInfrastructure(builder.Configuration);

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// OpenTelemetry
if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
{
    builder.Services.AddOpenTelemetry().UseAzureMonitor();
}

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddSource(DiagnosticsConfig.ServiceName)
        .AddAspNetCoreInstrumentation());

var app = builder.Build();

// Middleware pipeline
app.UseMiddleware<CorrelationIdMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Endpoints
app.MapAllEndpoints();

app.Run();

// Make Program accessible for integration tests
public partial class Program { }
