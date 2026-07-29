
using Core.Queries;
using API.DTOs;

namespace Core.Interfaces;

public interface IBookService
{
    Task<BookPagingDto> GetBooksAsync(BookQueryParams bookQueryParams);
    Task<BookDto> GetBookByIdAsync(Guid id);
}