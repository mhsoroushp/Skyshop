using System.Reflection.PortableExecutable;
using Core.Models;
using Core.Queries;


namespace Core.Interfaces;

public interface IBookRepository
{
    Task<BookPaging> GetAllAsync(BookQueryParams bookQueryParams);
    Task<Book?> GetByIdAsync(Guid id);
    Task AddAsync(Book item);
    void UpdateAsync(Book item);
    Task DeleteAsync(Guid id);
    Task<bool> SaveChangesAsync();
}
