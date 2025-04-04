namespace Example.SharedKernel.Models;

public class PagedList<T>
{
    public PagedList(List<T> items, int page, int pageSize, int totalCount, int queryCount)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
        QueryCount = queryCount;
    }

    public List<T> Items { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public int QueryCount { get; }
    public bool HasNextPage => (Page * PageSize) < QueryCount;
    public bool HasPreviousPage => Page > 1;
    public string? NextPageLink { get; set; }
    public string? PreviousPageLink { get; set; }
}