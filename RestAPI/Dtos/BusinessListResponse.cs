namespace RestAPI.Dtos;

public class BusinessListResponse
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public long Total { get; set; }
    public List<BusinessResponse> Businesses { get; set; } = new();
}
