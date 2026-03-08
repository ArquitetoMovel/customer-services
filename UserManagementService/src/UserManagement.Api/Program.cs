using MongoDB.Driver;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Resources;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using UserManagement.Application.Services;
using UserManagement.Application.UseCases;
using UserManagement.Domain.Ports;
using UserManagement.Infrastructure.MessageBroker;
using UserManagement.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

builder.Services.AddControllers();

var compositeTextMapPropagator = new CompositeTextMapPropagator(new TextMapPropagator[]
{
    new TraceContextPropagator(),
    new BaggagePropagator()
});

Sdk.SetDefaultTextMapPropagator(compositeTextMapPropagator);

var otelCollectorUri = new Uri(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] ?? "http://localhost:9317");

// Register services
builder.Services.AddSingleton<IMongoClient>(sp =>
    new MongoClient(builder.Configuration.GetConnectionString("MongoConnection")));
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IMongoClient>().GetDatabase("AttendanceTicketDB"));
builder.Services.AddSingleton<IMessageBrokerService, RabbitMqService>();
builder.Services.AddScoped<IUnitOfWork, MongoUnitOfWork>();
builder.Services.AddScoped<IAttendanceTicketRepository, MongoAttendanceTicketRepository>();

builder.Services.AddScoped<IAttendanceTicketService, AttendanceTicketService>();
builder.Services.AddScoped<GenerateAttendanceTicketUseCase>();
builder.Services.AddScoped<GetNextAttendanceTicketUseCase>();

// Configure OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(serviceName: builder.Environment.ApplicationName))
    .WithTracing(tracing => tracing.AddSource(builder.Environment.ApplicationName)
        .AddHttpClientInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddRabbitMQInstrumentation()
        .AddOtlpExporter(opt =>
            opt.Endpoint = otelCollectorUri)
    )
    .WithMetrics(metrics => metrics
        .AddMeter("UserManagement.Api")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(opt =>
            opt.Endpoint = otelCollectorUri
        )
    );

// Configure logging with OpenTelemetry.
builder.Logging.AddOpenTelemetry(options =>
{
    options.SetResourceBuilder(ResourceBuilder.CreateDefault()
        .AddService(builder.Environment.ApplicationName));
    options.IncludeFormattedMessage = true;
    options.ParseStateValues = true;
    options.IncludeScopes = true;
    options.AddOtlpExporter(exporter => exporter.Endpoint = otelCollectorUri);
});

var app = builder.Build();

app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

await app.RunAsync();
