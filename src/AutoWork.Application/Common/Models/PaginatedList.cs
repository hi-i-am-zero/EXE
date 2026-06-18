namespace AutoWork.Application.Common.Models;

public class PaginatedList<T>
{
    public IReadOnlyList<T> Items { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public int TotalPages { get; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PaginatedList(IReadOnlyList<T> items, int count, int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = count;
        TotalPages = pageSize > 0 ? (int)Math.Ceiling(count / (double)pageSize) : 0;
        Items = items;
    }

    public static PaginatedList<T> Create(IEnumerable<T> source, int count, int pageNumber, int pageSize)
    {
        return new PaginatedList<T>(source.ToList(), count, pageNumber, pageSize);
    }
}
