using BusinessService.Dtos;
using BusinessService.Infrastructure;

namespace BusinessService.Application;

public class BusinessService
{
    private readonly BusinessRepository _repo;

    public BusinessService(BusinessRepository repo)
    {
        _repo = repo;
    }

    public async Task<BusinessDto?> GetBusinessByIdAsync(long id)
    {
        var entity = await _repo.GetBusinessByIdAsync(id);
        return entity == null ? null : BusinessMapper.ToDto(entity);
    }
}
