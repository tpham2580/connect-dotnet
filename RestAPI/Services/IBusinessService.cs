using RestAPI.Dtos;

namespace RestAPI.Services;

public interface IBusinessService
{
    Task<BusinessListResponse> ListAsync(long after, int pageSize, CancellationToken cancellationToken);
    Task<BusinessResponse?> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task<BusinessResponse> CreateAsync(BusinessRequest request, CancellationToken cancellationToken);
    Task<BusinessResponse?> UpdateAsync(long id, BusinessRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken);
}
