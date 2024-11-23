using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Ports;
using NotificationService.Infrastructure.MessageBroker;
using NotificationService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("NotificationDb")));

builder.Services.AddSingleton<IMessageBroker, RabbitMqMessageBroker>();
builder.Services.AddHostedService<NotificationService.Application.NotificationService>();

builder.Services.AddScoped<IAttendanceTicketRepository, AttendanceTicketRepository>();


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

