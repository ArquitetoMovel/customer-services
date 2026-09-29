using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Ports;
using NotificationService.Infrastructure.MessageBroker;
using NotificationService.Infrastructure.Persistence;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

var compositeTextMapPropagator = new CompositeTextMapPropagator(new TextMapPropagator[]
{
    new TraceContextPropagator(),
    new BaggagePropagator()
});

Sdk.SetDefaultTextMapPropagator(compositeTextMapPropagator);

// Add services to the container.

builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("NotificationDb")));

builder.Services.AddSingleton<IMessageBroker, RabbitMqMessageBroker>();
builder.Services.AddHostedService<NotificationService.Application.NotificationService>();

builder.Services.AddScoped<IAttendanceTicketRepository, AttendanceTicketRepository>();

var otelCollectorUri = new Uri(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] ?? "http://localhost:9317");

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(serviceName: builder.Environment.ApplicationName))
    .WithTracing(tracing => tracing.AddSource(builder.Environment.ApplicationName)
        .AddAspNetCoreInstrumentation()
        .AddRabbitMQInstrumentation()
        .AddNpgsql()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = otelCollectorUri;
        })
    )
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter(options => options.Endpoint = otelCollectorUri));

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<NotificationDbContext>().Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
await app.RunAsync();
