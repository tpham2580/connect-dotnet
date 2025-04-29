using BusinessService.Dtos;
using BusinessService.Infrastructure;
using Grpc.BusinessService;

namespace BusinessService.Application;

public static class BusinessMapper
{
    public static BusinessDto ToDto(BusinessEntity entity) => new BusinessDto
    {
        Id = entity.BusinessId,
        Name = entity.Name,
        Address = entity.Address,
        City = entity.City,
        State = entity.State,
        Country = entity.Country,
        Latitude = entity.Latitude,
        Longitude = entity.Longitude
    };

    public static Business ToGrpc(BusinessDto dto) => new Business
    {
        Id = dto.Id,
        Name = dto.Name,
        Address = dto.Address,
        City = dto.City,
        State = dto.State,
        Country = dto.Country,
        Latitude = dto.Latitude,
        Longitude = dto.Longitude
    };
}
