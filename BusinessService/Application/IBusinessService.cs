using BusinessService.Models;

namespace BusinessService.Application;

public interface IBusinessService
{
    Task<BusinessModel?> GetBusinessByIdAsync(long id);
    Task<List<BusinessModel>> GetAllBusinessesByIdsAsync(List<long> ids);
    Task<(List<BusinessModel> Businesses, long Total)> GetBusinessesAsync(int limit, int offset);
    Task<BusinessModel?> CreateBusinessAsync(BusinessModel business);
    Task<BusinessModel?> UpdateBusinessAsync(BusinessModel business);
    Task<bool> DeleteBusinessByIdAsync(long id);
}
