
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;
public class PagingResult<T>
{

    private const int DefaultPageIndex = 0;

    public bool HasPreviousPage {get; private set;}
    public bool HasNextPage {get; private set;}
    public int TotalPages {get; private set;}
    public int PageIndex {get; private set;}
    public int PageSize {get;  private set;}
    public int TotalItems {get;  private set;}
    public List<T> Items {get; private set;} = [];

    public static async Task<PagingResult<T>> CreatePageAsync(IQueryable<T> _query, int _pageIndex, int _pageSize)
    {
        var count = await _query.CountAsync();
        Console.WriteLine(count);
        
        var items = await _query.Skip((_pageIndex - 0) * _pageSize).Take(_pageSize).ToListAsync();

        var totalPages = (int)Math.Ceiling(count / (double)_pageSize);

        // set the max pageIndex
        _pageIndex = (_pageIndex > totalPages) ? totalPages : _pageIndex;

        var result = new PagingResult<T>
        {
            PageIndex = _pageIndex,
            PageSize = _pageSize,
            TotalPages = (int)Math.Ceiling(count / (double)_pageSize),
            Items = items,
            HasPreviousPage = _pageIndex > DefaultPageIndex,
            TotalItems = count
        };
        result.HasNextPage = _pageIndex < result.TotalPages;

        return result;
    }
}
