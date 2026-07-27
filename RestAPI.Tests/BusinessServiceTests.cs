using Microsoft.Extensions.Logging.Abstractions;
using RestAPI.Dtos;
using GrpcBusiness = Grpc.BusinessService;

namespace RestAPI.Tests;

/// <summary>
/// Unit tests for the RestAPI <see cref="RestAPI.Services.BusinessService"/> wrapper,
/// exercising the DTO &lt;-&gt; gRPC mapping and RpcException handling directly against
/// <see cref="FakeBusinessServiceClient"/> (no HTTP pipeline).
/// </summary>
public class BusinessServiceTests
{
    private static GrpcBusiness.Business Seed(long id, string name) => new GrpcBusiness.Business
    {
        Id = id,
        Name = name,
        Address = "123 Main St",
        City = "Seattle",
        State = "WA",
        Country = "USA",
        Latitude = 47.6,
        Longitude = -122.3
    };

    private static BusinessRequest ValidRequest(string name = "New Business") => new BusinessRequest
    {
        Name = name,
        Address = "456 Market Ave",
        City = "Portland",
        State = "OR",
        Country = "USA",
        Latitude = 45.52,
        Longitude = -122.68
    };

    private static RestAPI.Services.BusinessService CreateSut(params GrpcBusiness.Business[] seed)
    {
        var client = new FakeBusinessServiceClient(seed.Length == 0 ? null : seed);
        return new RestAPI.Services.BusinessService(
            NullLogger<RestAPI.Services.BusinessService>.Instance, client);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        var sut = CreateSut();

        var result = await sut.GetByIdAsync(404);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_KnownId_MapsAllFields()
    {
        var sut = CreateSut(Seed(1, "Alpha"));

        var result = await sut.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);
        Assert.Equal("Alpha", result.Name);
        Assert.Equal("Seattle", result.City);
        Assert.Equal(47.6, result.Latitude, 3);
        Assert.Equal(-122.3, result.Longitude, 3);
    }

    [Fact]
    public async Task ListAsync_MapsPagingAndTotal()
    {
        var sut = CreateSut(Seed(1, "Alpha"), Seed(2, "Beta"), Seed(3, "Gamma"));

        var result = await sut.ListAsync(page: 2, pageSize: 2);

        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(3, result.Total);
        Assert.Single(result.Businesses);
        Assert.Equal(3, result.Businesses[0].Id);
    }

    [Fact]
    public async Task CreateAsync_ReturnsMappedResponseWithGeneratedId()
    {
        var sut = CreateSut();

        var result = await sut.CreateAsync(ValidRequest());

        Assert.True(result.Id > 0);
        Assert.Equal("New Business", result.Name);
        Assert.Equal("Portland", result.City);
    }

    [Fact]
    public async Task UpdateAsync_UnknownId_ReturnsNull()
    {
        var sut = CreateSut();

        var result = await sut.UpdateAsync(404, ValidRequest());

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_KnownId_ReturnsUpdatedResponse()
    {
        var sut = CreateSut(Seed(1, "Alpha"));

        var result = await sut.UpdateAsync(1, ValidRequest("Renamed"));

        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);
        Assert.Equal("Renamed", result.Name);
    }

    [Fact]
    public async Task DeleteAsync_UnknownId_ReturnsFalse()
    {
        var sut = CreateSut();

        Assert.False(await sut.DeleteAsync(404));
    }

    [Fact]
    public async Task DeleteAsync_KnownId_ReturnsTrue()
    {
        var sut = CreateSut(Seed(1, "Alpha"));

        Assert.True(await sut.DeleteAsync(1));
    }
}
