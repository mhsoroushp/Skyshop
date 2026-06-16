using Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class BookContext: DbContext
{
    public BookContext(DbContextOptions<BookContext> options)
        : base(options)
    {
    }

    public DbSet<Book> Books { get; set; } = null!;
}
