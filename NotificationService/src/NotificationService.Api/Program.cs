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

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(serviceName: builder.Environment.ApplicationName))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddRabbitMQInstrumentation()
        .AddNpgsql()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("http://otel-collector:9317");
        })
    )
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
    );

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.Run();

