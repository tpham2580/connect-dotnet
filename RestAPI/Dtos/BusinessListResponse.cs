namespace RestAPI.Dtos;

public sealed class BusinessListResponse
{
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required long Total { get; init; }
    public required IReadOnlyList<BusinessResponse> Businesses { get; init; }
}
