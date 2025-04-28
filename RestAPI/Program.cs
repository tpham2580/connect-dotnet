using Microsoft.AspNetCore.Diagnostics;
using Microsoft.OpenApi.Models;
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

// Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddScoped<ILocationSearchService, LocationSearchService>();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo { Title = "Mobile BFF API", Version = "v1" });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts(); // Strict Transport Security header
}

// Middleware
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Global error endpoint
app.Map("/error", (HttpContext context) =>
{
    var error = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    return Results.Problem(error?.Message);
});

app.Run();
