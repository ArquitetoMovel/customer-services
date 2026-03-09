
var builder = DistributedApplication.CreateBuilder(args);
builder.AddProject<Projects.CustomerManagementApp>("CustomerManagementApp");
builder.Build().Run();