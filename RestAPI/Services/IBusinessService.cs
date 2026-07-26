using RestAPI.Dtos;

namespace RestAPI.Services;

public interface IBusinessService
{
    Task<BusinessListResponse> ListAsync(int page, int pageSize);
    Task<BusinessResponse?> GetByIdAsync(long id);
    Task<BusinessResponse> CreateAsync(BusinessRequest request);
}
