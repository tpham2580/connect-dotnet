using System.Net;
using System.Net.Http.Json;
using RestAPI.Dtos;
using GrpcBusiness = Grpc.BusinessService;

namespace RestAPI.Tests;

public class BusinessControllerTests
{
    private static GrpcBusiness.Business SeedBusiness(long id, string name) => new GrpcBusiness.Business
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

    private static BusinessRequest ValidRequest() => new BusinessRequest
    {
        Name = "New Business",
        Address = "456 Market Ave",
        City = "Portland",
        State = "OR",
        Country = "USA",
        Latitude = 45.52,
        Longitude = -122.68
    };

    [Fact]
    public async Task GetBusinesses_ReturnsPagedResults()
    {
        await using var factory = new BusinessApiFactory(new[]
        {
            SeedBusiness(1, "Alpha"),
            SeedBusiness(2, "Beta")
        });
        var client = factory.CreateClient();

        var response = await client.GetAsync("/businesses?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<BusinessListResponse>();
        Assert.NotNull(body);
        Assert.Equal(2, body!.Total);
        Assert.Equal(1, body.Page);
        Assert.Equal(10, body.PageSize);
        Assert.Equal(2, body.Businesses.Count);
    }

    [Fact]
    public async Task GetBusinesses_HonorsPaging()
    {
        await using var factory = new BusinessApiFactory(new[]
        {
            SeedBusiness(1, "Alpha"),
            SeedBusiness(2, "Beta"),
            SeedBusiness(3, "Gamma")
        });
        var client = factory.CreateClient();

        var response = await client.GetAsync("/businesses?page=2&pageSize=2");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<BusinessListResponse>();
        Assert.NotNull(body);
        Assert.Equal(3, body!.Total);
        Assert.Single(body.Businesses);
        Assert.Equal(3, body.Businesses[0].Id);
    }

    [Fact]
    public async Task CreateBusiness_WithValidPayload_Returns201AndIsRetrievable()
    {
        await using var factory = new BusinessApiFactory();
        var client = factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync("/businesses", ValidRequest());

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createResponse.Headers.Location);

        var created = await createResponse.Content.ReadFromJsonAsync<BusinessResponse>();
        Assert.NotNull(created);
        Assert.True(created!.Id > 0);
        Assert.Equal("New Business", created.Name);

        var getResponse = await client.GetAsync($"/businesses/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<BusinessResponse>();
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched!.Id);
        Assert.Equal("New Business", fetched.Name);
    }

    [Fact]
    public async Task CreateBusiness_WithInvalidPayload_Returns400()
    {
        await using var factory = new BusinessApiFactory();
        var client = factory.CreateClient();

        var invalid = ValidRequest();
        invalid.Name = string.Empty;   // violates [Required]
        invalid.Latitude = 999;        // violates [Range(-90, 90)]

        var response = await client.PostAsJsonAsync("/businesses", invalid);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetBusinessById_WithUnknownId_Returns404()
    {
        await using var factory = new BusinessApiFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/businesses/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
