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

        await sut.ListBusinesses(new ListBusinessesRequest { Limit = 0, Offset = 0 }, null!);

        Assert.Equal(100, fake.LastLimit);
    }

    [Fact]
    public async Task ListBusinesses_CapsLimit_AtMax()
    {
        var fake = new FakeBusinessAppService();
        var sut = CreateSut(fake);

        await sut.ListBusinesses(new ListBusinessesRequest { Limit = 500, Offset = 0 }, null!);

        Assert.Equal(100, fake.LastLimit);
    }

    [Fact]
    public async Task ListBusinesses_ClampsNegativeOffset_ToZero()
    {
        var fake = new FakeBusinessAppService();
        var sut = CreateSut(fake);

        await sut.ListBusinesses(new ListBusinessesRequest { Limit = 10, Offset = -5 }, null!);

        Assert.Equal(0, fake.LastOffset);
    }

    [Fact]
    public async Task ListBusinesses_PassesValidPaging_AndMapsTotal()
    {
        var fake = new FakeBusinessAppService
        {
            ListResult = (new List<BusinessModelEntity> { Model(1) }, 7)
        };
        var sut = CreateSut(fake);

        var response = await sut.ListBusinesses(new ListBusinessesRequest { Limit = 50, Offset = 10 }, null!);

        Assert.Equal(50, fake.LastLimit);
        Assert.Equal(10, fake.LastOffset);
        Assert.Equal(7, response.Total);
        Assert.Single(response.Businesses);
        Assert.Equal(1, response.Businesses[0].Id);
    }

    [Fact]
    public async Task UpdateBusiness_Throws_NotFound_WhenServiceReturnsNull()
    {
        var fake = new FakeBusinessAppService { UpdateResult = null };
        var sut = CreateSut(fake);

        var ex = await Assert.ThrowsAsync<RpcException>(() =>
            sut.UpdateBusiness(new UpdateBusinessRequest { Business = ValidGrpcBusiness(5) }, null!));

        Assert.Equal(StatusCode.NotFound, ex.StatusCode);
    }

    [Fact]
    public async Task DeleteBusiness_Throws_NotFound_WhenServiceReturnsFalse()
    {
        var fake = new FakeBusinessAppService { DeleteResult = false };
        var sut = CreateSut(fake);

        var ex = await Assert.ThrowsAsync<RpcException>(() =>
            sut.DeleteBusiness(new BusinessByIdRequest { Id = 9 }, null!));

        Assert.Equal(StatusCode.NotFound, ex.StatusCode);
    }
}
