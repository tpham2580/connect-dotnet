namespace RestAPI.Dtos;

public sealed class BusinessListResponse
{
    public required int PageSize { get; init; }
    public required long Total { get; init; }
    public required bool HasMore { get; init; }

    /// <summary>
    /// Value to pass back as <c>after</c> to fetch the next page. Null when there is no next page.
    /// </summary>
    public long? NextCursor { get; init; }

    public required IReadOnlyList<BusinessResponse> Businesses { get; init; }
}
