using MongoDB.Driver;
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

var app = builder.Build();

app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers()
    .WithOpenApi();

app.Run();
