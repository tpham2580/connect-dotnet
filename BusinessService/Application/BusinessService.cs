using BusinessService.Models;
using BusinessService.Infrastructure;

namespace BusinessService.Application;

public class BusinessService
{
    private readonly BusinessRepository _repo;
    private readonly ILogger<BusinessService> _log;

    public BusinessService(BusinessRepository repo, ILogger<BusinessService> log)
    {
        _repo = repo;
        _log = log;
    }

    public async Task<BusinessModel?> GetBusinessByIdAsync(long id)
    {
        var response = await _repo.GetBusinessByIdAsync(id);
        return response;
    }

    public async Task<BusinessModel?> CreateBusinessAsync(BusinessModel business)
    {
        _log.LogInformation("Received Business Model: \n{@business}", business);
        var response = await _repo.CreateBusinessAsync(business);
        return response;
    }

    public async Task<BusinessModel?> UpdateBusinessAsync(BusinessModel business)
    {
        _log.LogInformation("Received Business Model: \n{@business}", business);
        var response = await _repo.UpdateBusinessAsync(business);
        return response;
    }

}
