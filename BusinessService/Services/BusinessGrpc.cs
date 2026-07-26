using Grpc.Core;
using Grpc.BusinessService;
using BusinessService.Application;

namespace BusinessService.Services;

public class BusinessGrpc : Grpc.BusinessService.BusinessService.BusinessServiceBase
{
    private const int DefaultPageSize = 100;
    private const int MaxPageSize = 100;

    private readonly ILogger<BusinessGrpc> _log;
    private readonly BusinessService.Application.BusinessService _service;

    public BusinessGrpc(ILogger<BusinessGrpc> log, BusinessService.Application.BusinessService service)
    {
        _log = log;
        _service = service;
    }

    public override async Task<BusinessResponse> GetBusinessById(BusinessByIdRequest request, ServerCallContext context)
    {
        var response = await _service.GetBusinessByIdAsync(request.Id);

        if (response == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Business with ID {request.Id} not found."));
        }

        return new BusinessResponse
        {
            Business = BusinessMapper.ToGrpc(response)
        };
    }

    public override async Task<GetAllBusinessesByIdsResponse> GetAllBusinessesByIds(BusinessesByIdsRequest request, ServerCallContext context)
    {
        if (request.Ids == null || request.Ids.Count == 0)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "List of business IDs must not be empty."));
        }

        var response = await _service.GetAllBusinessesByIdsAsync(request.Ids.ToList());

        return new GetAllBusinessesByIdsResponse
        {
            Businesses = { response.Select(BusinessMapper.ToGrpc) }
        };
    }

    public override async Task<ListBusinessesResponse> ListBusinesses(ListBusinessesRequest request, ServerCallContext context)
    {
        var limit = request.Limit <= 0 ? DefaultPageSize : Math.Min(request.Limit, MaxPageSize);
        var offset = request.Offset < 0 ? 0 : request.Offset;

        var (businesses, total) = await _service.GetBusinessesAsync(limit, offset);

        var response = new ListBusinessesResponse { Total = total };
        response.Businesses.AddRange(businesses.Select(BusinessMapper.ToGrpc));
        return response;
    }

    public override async Task<BusinessResponse> CreateBusiness(CreateBusinessRequest request, ServerCallContext context)
    {
        _log.LogInformation("Request received: \n{@request}", request);

        var business = BusinessMapper.ToBusinessModel(request);

        _log.LogInformation("Request converted to Business Model: \n{@business}", business);

        var errors = Utils.Utils.IsValidBusinessInfo(business);
        if (errors.Any())
        {
            var message = string.Join("; ", errors);
            throw new RpcException(new Status(StatusCode.InvalidArgument, message));
        }

        var response = await _service.CreateBusinessAsync(business);

        if (response == null)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"Unable to create new Business."));
        }

        return new BusinessResponse
        {
            Business = BusinessMapper.ToGrpc(response)
        };
    }

    public override async Task<BusinessResponse> UpdateBusiness(UpdateBusinessRequest request, ServerCallContext context)
    {
        _log.LogInformation("Request received: \n{@request}", request);

        var business = BusinessMapper.ToBusinessModel(request);

        _log.LogInformation("Request converted to Business Model: \n{@business}", business);

        var errors = Utils.Utils.IsValidBusinessInfo(business, requireId: true);
        if (errors.Any())
        {
            var message = string.Join("; ", errors);
            throw new RpcException(new Status(StatusCode.InvalidArgument, message));
        }

        var response = await _service.UpdateBusinessAsync(business);

        if (response == null)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"Unable to update Business. Id not found."));
        }

        return new BusinessResponse
        {
            Business = BusinessMapper.ToGrpc(response)
        };
    }

    public override async Task<DeleteItemByIdResponse> DeleteBusiness(BusinessByIdRequest request, ServerCallContext context)
    {
        var response = await _service.DeleteBusinessByIdAsync(request.Id);

        if (!response)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Business with ID {request.Id} not found."));
        }

        return new DeleteItemByIdResponse
        {
            Success = true
        };
    }

}
