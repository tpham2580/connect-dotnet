using LocationService.Infrastructure;
using LocationService.Services;
using Serilog;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Logging
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/location-log-.txt",
                  rollingInterval: RollingInterval.Day,
                  retainedFileCountLimit: 14)
    .CreateLogger();
builder.Host.UseSerilog();

// Redis Multiplexer
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
{
    // resolves to "redis:6379" when run with the compose file below
    var cs = builder.Configuration.GetConnectionString("Redis")!;
    var redisPassword = builder.Configuration["REDIS_PASSWORD"];

    var options = ConfigurationOptions.Parse(cs);
    if (!string.IsNullOrEmpty(redisPassword))
    {
        options.Password = redisPassword;
    }

    return ConnectionMultiplexer.Connect(options);
});

// Services
builder.Services.AddSingleton<ILocationRepository, RedisLocationRepository>();
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

var app = builder.Build();
app.MapGrpcService<LocationGrpc>();

if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

app.MapGet("/", () => "LocationService gRPC running");
app.Run();
