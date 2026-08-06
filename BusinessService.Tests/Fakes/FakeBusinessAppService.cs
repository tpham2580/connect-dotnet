using BusinessService.Application;
using BusinessService.Models;

namespace BusinessService.Tests.Fakes;

/// <summary>
/// Hand-rolled stand-in for <see cref="IBusinessService"/> that records the
/// arguments passed by <c>BusinessGrpc</c> and returns configurable results,
/// so the gRPC handlers can be unit tested without a database.
/// </summary>
internal sealed class FakeBusinessAppService : IBusinessService
{
    public int? LastLimit { get; private set; }
    public int? LastOffset { get; private set; }
    public CancellationToken LastCancellationToken { get; private set; }

    public (List<BusinessModel> Businesses, long Total) ListResult { get; set; } = (new List<BusinessModel>(), 0);
    public BusinessModel? GetByIdResult { get; set; }
    public BusinessModel? CreateResult { get; set; }
    public BusinessModel? UpdateResult { get; set; }
    public bool DeleteResult { get; set; }

    public Task<BusinessModel?> GetBusinessByIdAsync(long id, CancellationToken cancellationToken) =>
        Task.FromResult(GetByIdResult);

    public Task<List<BusinessModel>> GetAllBusinessesByIdsAsync(
        List<long> ids,
        CancellationToken cancellationToken) =>
        Task.FromResult(new List<BusinessModel>());

    public Task<(List<BusinessModel> Businesses, long Total)> GetBusinessesAsync(
        int limit,
        int offset,
        CancellationToken cancellationToken)
    {
        LastLimit = limit;
        LastOffset = offset;
        LastCancellationToken = cancellationToken;
        return Task.FromResult(ListResult);
    }

    public Task<BusinessModel?> CreateBusinessAsync(
        BusinessModel business,
        CancellationToken cancellationToken) =>
        Task.FromResult(CreateResult);

    public Task<BusinessModel?> UpdateBusinessAsync(
        BusinessModel business,
        CancellationToken cancellationToken) =>
        Task.FromResult(UpdateResult);

    public Task<bool> DeleteBusinessByIdAsync(long id, CancellationToken cancellationToken) =>
        Task.FromResult(DeleteResult);
}
