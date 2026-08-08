using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using GrpcBusiness = Grpc.BusinessService;

namespace RestAPI.Tests;

/// <summary>
/// Boots the RestAPI in-memory and swaps the real BusinessService gRPC client
/// for <see cref="FakeBusinessServiceClient"/> so no live gRPC backend is required.
/// </summary>
public class BusinessApiFactory : WebApplicationFactory<Program>
{
    /// <summary>
    /// The fake gRPC backend the API runs against. Mutate it from a test to drive behaviour,
    /// e.g. set <see cref="FakeBusinessServiceClient.FailWith"/> to simulate a downstream failure.
    /// </summary>
    public FakeBusinessServiceClient Client { get; }

    public BusinessApiFactory(IEnumerable<GrpcBusiness.Business>? seed = null)
    {
        Client = new FakeBusinessServiceClient(seed);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Provide dummy gRPC endpoints so client registrations never throw during startup.
        builder.UseSetting("GrpcSettings:BusinessServiceUrl", "http://localhost:6001");
        builder.UseSetting("GrpcSettings:LocationServiceUrl", "http://localhost:6000");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<GrpcBusiness.BusinessService.BusinessServiceClient>();
            services.AddSingleton<GrpcBusiness.BusinessService.BusinessServiceClient>(Client);
        });
    }
}
