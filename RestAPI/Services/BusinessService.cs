using Grpc.Core;
using RestAPI.Dtos;
using GrpcBusiness = Grpc.BusinessService;

namespace RestAPI.Services;

public class BusinessService : IBusinessService
{
    private readonly ILogger<BusinessService> _logger;
    private readonly GrpcBusiness.BusinessService.BusinessServiceClient _client;

    public BusinessService(
        ILogger<BusinessService> logger,
        GrpcBusiness.BusinessService.BusinessServiceClient client)
    {
        _logger = logger;
        _client = client;
    }

    public async Task<BusinessListResponse> ListAsync(int page, int pageSize)
    {
        var offset = (page - 1) * pageSize;

        _logger.LogInformation("Listing businesses. Page: {Page}, PageSize: {PageSize}", page, pageSize);

        var grpcResponse = await _client.ListBusinessesAsync(new GrpcBusiness.ListBusinessesRequest
        {
            Limit = pageSize,
            Offset = offset
        });

        return new BusinessListResponse
        {
            Page = page,
            PageSize = pageSize,
            Total = grpcResponse.Total,
            Businesses = grpcResponse.Businesses.Select(ToResponse).ToList()
        };
    }

    public async Task<BusinessResponse?> GetByIdAsync(long id)
    {
        _logger.LogInformation("Fetching business by id: {Id}", id);

        try
        {
            var grpcResponse = await _client.GetBusinessByIdAsync(new GrpcBusiness.BusinessByIdRequest
            {
                Id = id
            });

            return ToResponse(grpcResponse.Business);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            _logger.LogInformation("Business not found. Id: {Id}", id);
            return null;
        }
    }

    public async Task<BusinessResponse> CreateAsync(BusinessRequest request)
    {
        _logger.LogInformation("Creating business: {Name}", request.Name);

        var grpcResponse = await _client.CreateBusinessAsync(new GrpcBusiness.CreateBusinessRequest
        {
            Business = new GrpcBusiness.newBusiness
            {
                Name = request.Name,
                Address = request.Address,
                City = request.City,
                State = request.State,
                Country = request.Country,
                Latitude = request.Latitude,
                Longitude = request.Longitude
            }
        });

        return ToResponse(grpcResponse.Business);
    }

    public async Task<BusinessResponse?> UpdateAsync(long id, BusinessRequest request)
    {
        _logger.LogInformation("Updating business. Id: {Id}", id);

        try
        {
            var grpcResponse = await _client.UpdateBusinessAsync(new GrpcBusiness.UpdateBusinessRequest
            {
                Business = new GrpcBusiness.Business
                {
                    Id = id,
                    Name = request.Name,
                    Address = request.Address,
                    City = request.City,
                    State = request.State,
                    Country = request.Country,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude
                }
            });

            return ToResponse(grpcResponse.Business);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            _logger.LogInformation("Business not found for update. Id: {Id}", id);
            return null;
        }
    }

    public async Task<bool> DeleteAsync(long id)
    {
        _logger.LogInformation("Deleting business. Id: {Id}", id);

        try
        {
            var grpcResponse = await _client.DeleteBusinessAsync(new GrpcBusiness.BusinessByIdRequest
            {
                Id = id
            });

            return grpcResponse.Success;
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            _logger.LogInformation("Business not found for delete. Id: {Id}", id);
            return false;
        }
    }

    private static BusinessResponse ToResponse(GrpcBusiness.Business business) => new BusinessResponse
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
}
