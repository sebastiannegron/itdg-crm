using Azure.Monitor.OpenTelemetry.AspNetCore;
using FluentValidation;
using Itdg.Crm.Api.Extensions;
using Itdg.Crm.Api.Hubs;
using Itdg.Crm.Api.Middlewares;
using Itdg.Crm.Api.Infrastructure.Extensions;
using Itdg.Crm.Api.Application.Abstractions;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Infrastructure (DbContext, Repositories)
builder.Services.AddInfrastructure(builder.Configuration);

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// NotificationHubContext (depends on SignalR registered in AddInfrastructure)
builder.Services.AddScoped<INotificationHubContext, NotificationHubContext>();

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

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<UserSyncMiddleware>();

// Endpoints
app.MapAllEndpoints();

// SignalR hubs
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();

// Make Program accessible for integration tests
public partial class Program { }
