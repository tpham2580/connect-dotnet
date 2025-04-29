using BusinessService.Services;
using BusinessService.Infrastructure;
using BusinessService.Application;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Logging
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/business-log-.txt",
             rollingInterval: RollingInterval.Day,
             retainedFileCountLimit: 14)
    .CreateLogger();
builder.Host.UseSerilog();

// Services
builder.Services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();
builder.Services.AddScoped<BusinessRepository>();
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

var app = builder.Build();
app.MapGrpcService<BusinessGrpc>();

if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

app.MapGet("/", () => "BusinessService gRPC running");
app.Run();
