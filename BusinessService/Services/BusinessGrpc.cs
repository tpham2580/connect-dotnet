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
        var entity = await _repo.GetBusinessByIdAsync(request.Id);

        if (entity == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Business with ID {request.Id} not found."));
        }

        var dto = BusinessMapper.ToDto(entity);

        return new BusinessResponse
        {
            Business = BusinessMapper.ToGrpc(dto)
        };
    }
}
