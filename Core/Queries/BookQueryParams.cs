namespace Core.Queries;

public class BookQueryParams : QueryParams
{
    private string? _searchText;
    public string? SearchText 
    {   
        get => _searchText?.ToLower(); 
        set => _searchText = value; 
    }
}