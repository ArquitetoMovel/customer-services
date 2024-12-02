
var builder = DistributedApplication.CreateBuilder(args);
//var rabbitmq = builder.AddRabbitMQ("messaging")
//    .WithManagementPlugin();
builder.AddProject<Projects.CustomerManagementApp>("CustomerManagementApp");
//    .WithReference(rabbitmq);
builder.Build().Run();