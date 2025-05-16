using Grpc.Core;
using Grpc.BusinessService;
using BusinessService.Application;
using BusinessService.Infrastructure;

namespace BusinessService.Services;

public class BusinessGrpc : Grpc.BusinessService.BusinessService.BusinessServiceBase
{
    private readonly ILogger<BusinessGrpc> _log;
    private readonly BusinessRepository _repo;

    public BusinessGrpc(ILogger<BusinessGrpc> log, BusinessRepository repo)
    {
        _log = log;
        _repo = repo;
    }

    public override async Task<BusinessResponse> GetBusinessById(BusinessByIdRequest request, ServerCallContext context)
    {
        var response = await _repo.GetBusinessByIdAsync(request.Id);

        if (response == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Business with ID {request.Id} not found."));
        }

        return new BusinessResponse
        {
            Business = BusinessMapper.ToGrpc(response)
        };
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

        var response = await _repo.CreateBusinessAsync(business);

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

        var response = await _repo.UpdateBusinessAsync(business);

        if (response == null)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"Unable to update Business. Id not found."));
        }

        return new BusinessResponse
        {
            Business = BusinessMapper.ToGrpc(response)
        };
    }
}
