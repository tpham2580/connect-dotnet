using BusinessService.Models;
using Grpc.BusinessService;

namespace BusinessService.Application;

public static class BusinessMapper
{
    #region Grpc to Model

    public static BusinessModel ToBusinessModel(Business business) => new BusinessModel
    {
        Id = business.Id,
        Name = business.Name,
        Address = business.Address,
        City = business.City,
        State = business.State,
        Country = business.Country,
        Latitude = business.Latitude,
        Longitude = business.Longitude
    };

    public static BusinessModel ToBusinessModel(CreateBusinessRequest request) => new BusinessModel
    {
        Name = request.Business.Name,
        Address = request.Business.Address,
        City = request.Business.City,
        State = request.Business.State,
        Country = request.Business.Country,
        Latitude = request.Business.Latitude,
        Longitude = request.Business.Longitude

    };

    public static BusinessModel ToBusinessModel(UpdateBusinessRequest request) => new BusinessModel
    {
        Id = request.Business.Id,
        Name = request.Business.Name,
        Address = request.Business.Address,
        City = request.Business.City,
        State = request.Business.State,
        Country = request.Business.Country,
        Latitude = request.Business.Latitude,
        Longitude = request.Business.Longitude

    };

    #endregion

    #region Model to Grpc

    public static Business ToGrpc(BusinessModel business) => new Business
    {
        Id = business.Id.HasValue ? business.Id.Value : 0,
        Name = business.Name,
        Address = business.Address,
        City = business.City,
        State = business.State,
        Country = business.Country,
        Latitude = business.Latitude,
        Longitude = business.Longitude
    };

    #endregion
}
