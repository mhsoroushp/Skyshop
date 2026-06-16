    
    namespace Core.Models;
    public class BookPaging
    {
        public bool HasPreviousPage {get; set;}
        public bool HasNextPage {get; set;}
        public int TotalPages {get; set;}
        public int PageIndex {get; set;}
        public int PageSize {get; set;}
        public int TotalItems {get; set;}
        public IEnumerable<Book> Items {get; set;} = [];
    }
    
