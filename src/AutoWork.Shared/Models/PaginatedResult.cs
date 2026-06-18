namespace AutoWork.Shared.Models;

public class PaginatedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];

    public int TotalCount { get; init; }

    public int PageNumber { get; init; }

    public int PageSize { get; init; }

    public int TotalPages =>
        PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;

    public static PaginatedResult<T> Create(
        IReadOnlyList<T> items,
        int totalCount,
        int pageNumber,
        int pageSize) =>
        new()
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

    public static PaginatedResult<T> Empty(int pageNumber = 1, int pageSize = 10) =>
        Create([], 0, pageNumber, pageSize);
}
