using CustomerManagementApp.Api;
using CustomerManagementApp.Broker;
using CustomerManagementDomain.Ports;
using CustomerManagementInfra.Broker;
using CustomerManagementInfra.Database;
using ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<UserTicketDbContext>("customer_db");
builder.AddRabbitMQClient(connectionName: "customer_broker");
builder.Services.AddScoped<IUserTicketRepository, UserTicketRepository>();
builder.Services.AddSingleton<ICustomerIntegrationBus, CustomerIntegrationBus>();
builder.Services.AddHostedService<CustomerIntegrationService>();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();
app.MapCustomerApi();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseHttpsRedirection();
app.Run();