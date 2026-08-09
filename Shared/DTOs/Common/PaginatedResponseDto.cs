namespace Shared.DTOs.Common;

public class PaginatedResponseDto<T>
{
    public IEnumerable<T> Data { get; set; } = [];

    public PaginationMeta Meta { get; set; } = new();
}

public class PaginationMeta
{
    public int Page { get; set; }
    public int Limit { get; set; }
    public int Total { get; set; }
    public int LastPage { get; set; }
}
