using Core.Interfaces;
using Core.Models;
using Core.Queries;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class BookRepository(BookContext context) : IBookRepository
{
    public async Task<bool> SaveChangesAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }

    public async Task AddAsync(Book item)
    {
        await context.Books.AddAsync(item);
    }

    public async Task DeleteAsync(Guid id)
    {
        var item = await context.Books.FindAsync(id);
        if (item != null)
        {
            context.Books.Remove(item);
        }
    }

    public async Task<BookPaging> GetAllAsync(BookQueryParams bookQueryParams)
    {
        var query = context.Books.AsQueryable();
        if(!string.IsNullOrWhiteSpace(bookQueryParams.SearchText))
        {
            query = query.Where(b => b.Author.Contains(bookQueryParams.SearchText));
        }

        var pageSize = bookQueryParams.PageSize;
        var pageNumber = bookQueryParams.PageIndex;
        var pagedResult = await PagingResult<Book>.CreatePageAsync(query, pageNumber, pageSize);

        var bookPaging = new BookPaging
        {
            HasPreviousPage = pagedResult.HasPreviousPage,
            HasNextPage = pagedResult.HasNextPage,
            TotalPages = pagedResult.TotalPages,
            PageIndex = pagedResult.PageIndex,
            PageSize = pagedResult.PageSize,
            TotalItems = pagedResult.TotalItems,
            Items = pagedResult.Items
        };

        return bookPaging;
    }

    public async Task<Book?> GetByIdAsync(Guid id)
    {
        return await context.Books.FirstOrDefaultAsync(b => b.Id == id);
    }

    public void UpdateAsync(Book item)
    {
        context.Entry(item).State = EntityState.Modified;
    }
}
