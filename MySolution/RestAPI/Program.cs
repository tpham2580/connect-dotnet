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

// Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddScoped<ILocationService, LocationService>();
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
