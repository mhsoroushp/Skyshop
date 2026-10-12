
using Core.Interfaces;
using Core.Queries;
using API.DTOs;
using Core.Entities;
namespace Infrastructure.Data;


public class BookService : IBookService
{
    private readonly IBookRepository _bookRepo;
    private readonly IBlobStorageService _blobService;
    private readonly IAppLogger<BookService> _logger;

    public BookService(
        IBookRepository bookRepository,
        IBlobStorageService blobStorageService,
        IAppLogger<BookService> logger
        )
    {
        _bookRepo = bookRepository;
        _blobService = blobStorageService;
        _logger = logger;
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
        // TODO: remove later
        _logger.LogInformation("(Hadi check): Fetching book with id {BookId}", id);
        var book = await _bookRepo.GetByIdAsync(id);
        if (book == null)
        {
            _logger.LogWarning("Book not found for id {BookId}", id);
            return null!;
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
            _logger.LogError(ex, "Error downloading image for book {BookId}", book.Id);
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