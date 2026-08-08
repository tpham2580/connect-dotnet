using System.Net;
using System.Net.Http.Json;
using Grpc.Core;
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

    private static BusinessRequest ValidRequest(
        string name = "New Business",
        double latitude = 45.52) => new BusinessRequest
    {
        Name = name,
        Address = "456 Market Ave",
        City = "Portland",
        State = "OR",
        Country = "USA",
        Latitude = latitude,
        Longitude = -122.68
    };

    [Fact]
    public async Task GetBusinesses_ReturnsFirstPage()
    {
        await using var factory = new BusinessApiFactory(new[]
        {
            SeedBusiness(1, "Alpha"),
            SeedBusiness(2, "Beta")
        });
        var client = factory.CreateClient();

        var response = await client.GetAsync("/v1/businesses?pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<BusinessListResponse>();
        Assert.NotNull(body);
        Assert.Equal(2, body!.Total);
        Assert.Equal(10, body.PageSize);
        Assert.Equal(2, body.Businesses.Count);
        Assert.False(body.HasMore);
        Assert.Null(body.NextCursor);
    }

    [Fact]
    public async Task GetBusinesses_WalksPagesUsingCursor()
    {
        await using var factory = new BusinessApiFactory(new[]
        {
            SeedBusiness(1, "Alpha"),
            SeedBusiness(2, "Beta"),
            SeedBusiness(3, "Gamma")
        });
        var client = factory.CreateClient();

        var firstResponse = await client.GetAsync("/v1/businesses?pageSize=2");
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        var first = await firstResponse.Content.ReadFromJsonAsync<BusinessListResponse>();
        Assert.NotNull(first);
        Assert.Equal(3, first!.Total);
        Assert.Equal(2, first.Businesses.Count);
        Assert.True(first.HasMore);
        Assert.Equal(2, first.NextCursor);

        var secondResponse = await client.GetAsync($"/v1/businesses?after={first.NextCursor}&pageSize=2");
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);

        var second = await secondResponse.Content.ReadFromJsonAsync<BusinessListResponse>();
        Assert.NotNull(second);
        Assert.Single(second!.Businesses);
        Assert.Equal(3, second.Businesses[0].Id);
        Assert.False(second.HasMore);
        Assert.Null(second.NextCursor);
    }

    [Theory]
    [InlineData("/v1/businesses?after=-1&pageSize=20")]
    [InlineData("/v1/businesses?pageSize=0")]
    [InlineData("/v1/businesses?pageSize=101")]
    public async Task GetBusinesses_WithInvalidPaging_Returns400(string requestUri)
    {
        await using var factory = new BusinessApiFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync(requestUri);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetBusinesses_WithCursorPastTheEnd_ReturnsEmptyPage()
    {
        await using var factory = new BusinessApiFactory(new[] { SeedBusiness(1, "Alpha") });
        var client = factory.CreateClient();

        var response = await client.GetAsync("/v1/businesses?after=1000");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<BusinessListResponse>();
        Assert.NotNull(body);
        Assert.Empty(body!.Businesses);
        Assert.False(body.HasMore);
        Assert.Null(body.NextCursor);
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

        var invalid = ValidRequest(name: string.Empty, latitude: 999);

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

        var update = ValidRequest(name: "Updated Business");

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

        var invalid = ValidRequest(name: string.Empty, latitude: 999);

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

    [Theory]
    [InlineData("GET")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    public async Task Routes_WithNonPositiveId_Return404_InsteadOfServerError(string method)
    {
        await using var factory = new BusinessApiFactory();
        var client = factory.CreateClient();

        foreach (var id in new[] { "0", "-1" })
        {
            var request = new HttpRequestMessage(new HttpMethod(method), $"/v1/businesses/{id}");
            if (method != "GET" && method != "DELETE")
            {
                request.Content = JsonContent.Create(ValidRequest());
            }

            var response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }

    [Theory]
    [InlineData(StatusCode.InvalidArgument, HttpStatusCode.BadRequest)]
    [InlineData(StatusCode.NotFound, HttpStatusCode.NotFound)]
    [InlineData(StatusCode.AlreadyExists, HttpStatusCode.Conflict)]
    [InlineData(StatusCode.PermissionDenied, HttpStatusCode.Forbidden)]
    [InlineData(StatusCode.Unavailable, HttpStatusCode.ServiceUnavailable)]
    [InlineData(StatusCode.DeadlineExceeded, HttpStatusCode.GatewayTimeout)]
    [InlineData(StatusCode.Internal, HttpStatusCode.InternalServerError)]
    public async Task DownstreamGrpcFailure_IsMappedToHttpStatus(
        StatusCode grpcStatus,
        HttpStatusCode expected)
    {
        await using var factory = new BusinessApiFactory();
        factory.Client.FailWith = new RpcException(new Status(grpcStatus, "upstream detail"));
        var client = factory.CreateClient();

        var response = await client.GetAsync("/v1/businesses");

        Assert.Equal(expected, response.StatusCode);
    }

    [Fact]
    public async Task DownstreamInvalidArgument_OnCreate_Returns400WithUpstreamDetail()
    {
        await using var factory = new BusinessApiFactory();
        factory.Client.FailWith = new RpcException(
            new Status(StatusCode.InvalidArgument, "Name is required."));
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/v1/businesses", ValidRequest());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Name is required.", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task DownstreamServerFailure_DoesNotLeakInternalDetail()
    {
        await using var factory = new BusinessApiFactory();
        factory.Client.FailWith = new RpcException(
            new Status(StatusCode.Unavailable, "connection refused to 10.0.0.5:6001"));
        var client = factory.CreateClient();

        var response = await client.GetAsync("/v1/businesses");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.DoesNotContain("10.0.0.5", await response.Content.ReadAsStringAsync());
    }
}
