using BusinessService.Models;
using BusinessService.Infrastructure;

namespace BusinessService.Application;

public class BusinessService : IBusinessService
{
    private readonly BusinessRepository _repo;
    private readonly ILogger<BusinessService> _log;

    public BusinessService(BusinessRepository repo, ILogger<BusinessService> log)
    {
        _repo = repo;
        _log = log;
    }

    public async Task<BusinessModel?> GetBusinessByIdAsync(long id, CancellationToken cancellationToken)
    {
        var response = await _repo.GetBusinessByIdAsync(id, cancellationToken);
        return response;
    }

    public async Task<List<BusinessModel>> GetAllBusinessesByIdsAsync(
        List<long> ids,
        CancellationToken cancellationToken)
    {
        return await _repo.GetAllBusinessesByIdsAsync(ids, cancellationToken);
    }

    public async Task<(List<BusinessModel> Businesses, long Total)> GetBusinessesAsync(
        int limit,
        int offset,
        CancellationToken cancellationToken)
    {
        return await _repo.GetBusinessesAsync(limit, offset, cancellationToken);
    }

    public async Task<BusinessModel?> CreateBusinessAsync(
        BusinessModel business,
        CancellationToken cancellationToken)
    {
        _log.LogInformation("Received Business Model: \n{@business}", business);
        var response = await _repo.CreateBusinessAsync(business, cancellationToken);
        return response;
    }

    public async Task<BusinessModel?> UpdateBusinessAsync(
        BusinessModel business,
        CancellationToken cancellationToken)
    {
        _log.LogInformation("Received Business Model: \n{@business}", business);
        var response = await _repo.UpdateBusinessAsync(business, cancellationToken);
        return response;
    }

    public async Task<bool> DeleteBusinessByIdAsync(long id, CancellationToken cancellationToken)
    {
        var response = await _repo.DeleteBusinessByIdAsync(id, cancellationToken);
        return response != false;
    }
}
