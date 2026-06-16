namespace Core.Queries;

public class QueryParams
{
    private const int MayPageSize = 10;
    private const int DefaultPageIndex = 0;

    private int _pageIndex;
    public int PageIndex 
    {
        get => _pageIndex;
        set => _pageIndex = (value < DefaultPageIndex)? DefaultPageIndex : value;
    }

    private int _pageSize = 5;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MayPageSize) ? MayPageSize : value;
    }
    
}