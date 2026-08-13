using BusinessService.Application;
using BusinessService.Services;
using BusinessService.Tests.Fakes;
using Grpc.BusinessService;
using Grpc.Core;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using BusinessModelEntity = BusinessService.Models.BusinessModel;

namespace BusinessService.Tests.UnitTests;

public class BusinessGrpcTests
{
    private static BusinessGrpc CreateSut(FakeBusinessAppService service) =>
        new BusinessGrpc(NullLogger<BusinessGrpc>.Instance, service);

    private static Business ValidGrpcBusiness(long id) => new Business
    {
        Id = id,
        Name = "Name",
        Address = "Addr",
        City = "City",
        State = "ST",
        Country = "USA",
        Latitude = 10,
        Longitude = 20
    };

    private static BusinessModelEntity Model(long id) => new BusinessModelEntity
    {
        Id = id,
        Name = "A",
        Address = "B",
        City = "C",
        State = "S",
        Country = "USA",
        Latitude = 1,
        Longitude = 2
    };

    [Fact]
    public async Task ListBusinesses_DefaultsLimit_WhenNonPositive()
    {
        var fake = new FakeBusinessAppService();
        var sut = CreateSut(fake);

        await sut.ListBusinesses(
            new ListBusinessesRequest { Limit = 0, After = 0 },
            TestServerCallContext.Create());

        Assert.Equal(100, fake.LastLimit);
    }

    [Fact]
    public async Task ListBusinesses_CapsLimit_AtMax()
    {
        var fake = new FakeBusinessAppService();
        var sut = CreateSut(fake);

        await sut.ListBusinesses(
            new ListBusinessesRequest { Limit = 500, After = 0 },
            TestServerCallContext.Create());

        Assert.Equal(100, fake.LastLimit);
    }

    [Fact]
    public async Task ListBusinesses_ClampsNegativeCursor_ToZero()
    {
        var fake = new FakeBusinessAppService();
        var sut = CreateSut(fake);

        await sut.ListBusinesses(
            new ListBusinessesRequest { Limit = 10, After = -5 },
            TestServerCallContext.Create());

        Assert.Equal(0, fake.LastAfter);
    }

    [Fact]
    public async Task ListBusinesses_PassesCursor_AndMapsTotal()
    {
        var fake = new FakeBusinessAppService
        {
            ListResult = (new List<BusinessModelEntity> { Model(1) }, 7, false)
        };
        var sut = CreateSut(fake);

        var response = await sut.ListBusinesses(
            new ListBusinessesRequest { Limit = 50, After = 10 },
            TestServerCallContext.Create());

        Assert.Equal(50, fake.LastLimit);
        Assert.Equal(10, fake.LastAfter);
        Assert.Equal(7, response.Total);
        Assert.Single(response.Businesses);
        Assert.Equal(1, response.Businesses[0].Id);
    }

    [Fact]
    public async Task ListBusinesses_SetsNextCursor_ToLastId_WhenMorePagesExist()
    {
        var fake = new FakeBusinessAppService
        {
            ListResult = (new List<BusinessModelEntity> { Model(1), Model(4) }, 9, true)
        };
        var sut = CreateSut(fake);

        var response = await sut.ListBusinesses(
            new ListBusinessesRequest { Limit = 2 },
            TestServerCallContext.Create());

        Assert.True(response.HasMore);
        Assert.Equal(4, response.NextCursor);
    }

    [Fact]
    public async Task ListBusinesses_LeavesNextCursorUnset_OnLastPage()
    {
        var fake = new FakeBusinessAppService
        {
            ListResult = (new List<BusinessModelEntity> { Model(1) }, 1, false)
        };
        var sut = CreateSut(fake);

        var response = await sut.ListBusinesses(
            new ListBusinessesRequest { Limit = 2 },
            TestServerCallContext.Create());

        Assert.False(response.HasMore);
        Assert.Equal(0, response.NextCursor);
    }

    [Fact]
    public async Task ListBusinesses_PropagatesCancellationToken()
    {
        var fake = new FakeBusinessAppService();
        var sut = CreateSut(fake);
        using var cancellationSource = new CancellationTokenSource();

        await sut.ListBusinesses(
            new ListBusinessesRequest { Limit = 10 },
            TestServerCallContext.Create(cancellationSource.Token));

        Assert.Equal(cancellationSource.Token, fake.LastCancellationToken);
    }

    [Fact]
    public async Task UpdateBusiness_Throws_NotFound_WhenServiceReturnsNull()
    {
        var fake = new FakeBusinessAppService { UpdateResult = null };
        var sut = CreateSut(fake);

        var ex = await Assert.ThrowsAsync<RpcException>(() =>
            sut.UpdateBusiness(
                new UpdateBusinessRequest { Business = ValidGrpcBusiness(5) },
                TestServerCallContext.Create()));

        Assert.Equal(StatusCode.NotFound, ex.StatusCode);
    }

    [Fact]
    public async Task CreateBusiness_Throws_InvalidArgument_WhenBusinessPayloadMissing()
    {
        var fake = new FakeBusinessAppService();
        var sut = CreateSut(fake);

        var ex = await Assert.ThrowsAsync<RpcException>(() =>
            sut.CreateBusiness(new CreateBusinessRequest(), TestServerCallContext.Create()));

        Assert.Equal(StatusCode.InvalidArgument, ex.StatusCode);
        Assert.Equal(0, fake.CreateCallCount);
    }

    [Fact]
    public async Task UpdateBusiness_Throws_InvalidArgument_WhenBusinessPayloadMissing()
    {
        var fake = new FakeBusinessAppService();
        var sut = CreateSut(fake);

        var ex = await Assert.ThrowsAsync<RpcException>(() =>
            sut.UpdateBusiness(new UpdateBusinessRequest(), TestServerCallContext.Create()));

        Assert.Equal(StatusCode.InvalidArgument, ex.StatusCode);
        Assert.Equal(0, fake.UpdateCallCount);
    }

    [Fact]
    public void BusinessMapper_Throws_ArgumentNullException_WhenCreateBusinessPayloadMissing()
    {
        Assert.Throws<ArgumentNullException>(() =>
            BusinessMapper.ToBusinessModel(new CreateBusinessRequest()));
    }

    [Fact]
    public void BusinessMapper_Throws_ArgumentNullException_WhenUpdateBusinessPayloadMissing()
    {
        Assert.Throws<ArgumentNullException>(() =>
            BusinessMapper.ToBusinessModel(new UpdateBusinessRequest()));
    }

    [Fact]
    public async Task DeleteBusiness_Throws_NotFound_WhenServiceReturnsFalse()
    {
        var fake = new FakeBusinessAppService { DeleteResult = false };
        var sut = CreateSut(fake);

        var ex = await Assert.ThrowsAsync<RpcException>(() =>
            sut.DeleteBusiness(
                new BusinessByIdRequest { Id = 9 },
                TestServerCallContext.Create()));

        Assert.Equal(StatusCode.NotFound, ex.StatusCode);
    }
}
