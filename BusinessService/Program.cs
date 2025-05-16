using BusinessService.Services;
using BusinessService.Infrastructure;
using BusinessService.Application;
using Serilog;
using Serilog.Enrichers.WithCaller;

var builder = WebApplication.CreateBuilder(args);

// Logging
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithCaller(includeFileInfo: true)
    .WriteTo.Console(
        outputTemplate:
        "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] " +
        "{SourceContext}.{CallerMemberName} :: {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("Logs/business-log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14,
        outputTemplate:
        "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] " +
        "{SourceContext}.{CallerMemberName} :: {Message:lj}{NewLine}{Exception}")
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
