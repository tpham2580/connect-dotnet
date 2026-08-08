using BusinessService.Models;

namespace BusinessService.Application;

public interface IBusinessService
{
    Task<BusinessModel?> GetBusinessByIdAsync(long id, CancellationToken cancellationToken);
    Task<List<BusinessModel>> GetAllBusinessesByIdsAsync(List<long> ids, CancellationToken cancellationToken);
    Task<(List<BusinessModel> Businesses, long Total, bool HasMore)> GetBusinessesAsync(
        int limit,
        long after,
        CancellationToken cancellationToken);
    Task<BusinessModel?> CreateBusinessAsync(BusinessModel business, CancellationToken cancellationToken);
    Task<BusinessModel?> UpdateBusinessAsync(BusinessModel business, CancellationToken cancellationToken);
    Task<bool> DeleteBusinessByIdAsync(long id, CancellationToken cancellationToken);
}
