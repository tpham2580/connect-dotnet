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

        var response = await client.GetAsync("/v1/businesses?page=1&pageSize=10");

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

        var response = await client.GetAsync("/v1/businesses?page=2&pageSize=2");

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

        var createResponse = await client.PostAsJsonAsync("/v1/businesses", ValidRequest());

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createResponse.Headers.Location);

        var created = await createResponse.Content.ReadFromJsonAsync<BusinessResponse>();
        Assert.NotNull(created);
        Assert.True(created!.Id > 0);
        Assert.Equal("New Business", created.Name);

        var getResponse = await client.GetAsync($"/v1/businesses/{created.Id}");
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

        var response = await client.PostAsJsonAsync("/v1/businesses", invalid);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetBusinessById_WithUnknownId_Returns404()
    {
        await using var factory = new BusinessApiFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/v1/businesses/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBusiness_WithValidPayload_Returns200AndPersists()
    {
        await using var factory = new BusinessApiFactory(new[] { SeedBusiness(1, "Alpha") });
        var client = factory.CreateClient();

        var update = ValidRequest();
        update.Name = "Updated Business";

        var putResponse = await client.PutAsJsonAsync("/v1/businesses/1", update);

        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);

        var updated = await putResponse.Content.ReadFromJsonAsync<BusinessResponse>();
        Assert.NotNull(updated);
        Assert.Equal(1, updated!.Id);
        Assert.Equal("Updated Business", updated.Name);

        var getResponse = await client.GetAsync("/v1/businesses/1");
        var fetched = await getResponse.Content.ReadFromJsonAsync<BusinessResponse>();
        Assert.Equal("Updated Business", fetched!.Name);
    }

    [Fact]
    public async Task UpdateBusiness_WithUnknownId_Returns404()
    {
        await using var factory = new BusinessApiFactory();
        var client = factory.CreateClient();

        var response = await client.PutAsJsonAsync("/v1/businesses/99999", ValidRequest());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBusiness_WithInvalidPayload_Returns400()
    {
        await using var factory = new BusinessApiFactory(new[] { SeedBusiness(1, "Alpha") });
        var client = factory.CreateClient();

        var invalid = ValidRequest();
        invalid.Name = string.Empty;   // violates [Required]
        invalid.Latitude = 999;        // violates [Range(-90, 90)]

        var response = await client.PutAsJsonAsync("/v1/businesses/1", invalid);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteBusiness_WithExistingId_Returns204AndIsGone()
    {
        await using var factory = new BusinessApiFactory(new[] { SeedBusiness(1, "Alpha") });
        var client = factory.CreateClient();

        var deleteResponse = await client.DeleteAsync("/v1/businesses/1");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await client.GetAsync("/v1/businesses/1");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteBusiness_WithUnknownId_Returns404()
    {
        await using var factory = new BusinessApiFactory();
        var client = factory.CreateClient();

        var response = await client.DeleteAsync("/v1/businesses/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
