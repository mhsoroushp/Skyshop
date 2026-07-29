
using Core.Interfaces;
using Core.Queries;
using API.DTOs;
using Core.Entities;
namespace Infrastructure.Data;


public class BookService : IBookService
{
    private readonly IBookRepository _bookRepo;
    private readonly IBlobStorageService _blobService;

    public BookService(
        IBookRepository bookRepository,
        IBlobStorageService blobStorageService
        )
    {
        _bookRepo = bookRepository;
        _blobService = blobStorageService;
    }

    public async Task<BookPagingDto> GetBooksAsync(BookQueryParams bookQueryParams)
    {
        var bookPaging = await _bookRepo.GetAllAsync(bookQueryParams);

        var bookPagingDto = new BookPagingDto
        {
            HasPreviousPage = bookPaging.HasPreviousPage,
            HasNextPage = bookPaging.HasNextPage,
            TotalPages = bookPaging.TotalPages,
            PageIndex = bookPaging.PageIndex,
            PageSize = bookPaging.PageSize,
            TotalItems = bookPaging.TotalItems,
            Items = await Task.WhenAll(bookPaging.Items.Select(async book => await ToBookDto(book)))
        };
        return bookPagingDto;
    }

    public async Task<BookDto> GetBookByIdAsync(Guid id)
    {
        var book = await _bookRepo.GetByIdAsync(id);
        if (book == null)
        {
            // return NotFound(new {message = $"Book with id {id} not found."});
            return null;
        }

        return await ToBookDto(book);
    }

    private async Task<BookDto> ToBookDto(Book book)
    {
        string CoverImageBase64 = string.Empty;

        try
        {
            var imageBytes = await _blobService.DownloadImageAsBytesAsync(book.CoverImageUrl ?? string.Empty);
            CoverImageBase64 = Convert.ToBase64String(imageBytes);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading image for book {book.Id}: {ex.Message}");
        }

        var bookDto = new BookDto
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            Description = book.Description,
            Price = book.Price,
            CoverImageBase64 = CoverImageBase64
        };

        return bookDto;
    }

}