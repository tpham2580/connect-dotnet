using Microsoft.OpenApi.Models;
using RestAPI.Infrastructure;
using RestAPI.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Logging
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/bff-log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// gRPC Client
builder.Services
    .AddGrpcClient<Grpc.LocationService.LocationService.LocationServiceClient>(options =>
    {
        options.Address = new Uri(builder.Configuration["GrpcSettings:LocationServiceUrl"]
                                ?? throw new Exception("Configuration Not Found"));
    });

builder.Services
    .AddGrpcClient<Grpc.BusinessService.BusinessService.BusinessServiceClient>(options =>
    {
        options.Address = new Uri(builder.Configuration["GrpcSettings:BusinessServiceUrl"]
                                ?? throw new Exception("Configuration Not Found"));
    });

// Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<RpcExceptionHandler>();
builder.Services.AddScoped<ILocationSearchService, LocationSearchService>();
builder.Services.AddScoped<IBusinessService, BusinessService>();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo { Title = "Mobile BFF API", Version = "v1" });
});

var app = builder.Build();

// Runs in every environment so downstream gRPC failures map to accurate status codes
// and internal exception messages are never echoed to callers.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts(); // Strict Transport Security header
}

// Middleware
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
