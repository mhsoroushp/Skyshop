using API.DTOs;
using Core.Models;

namespace API.Extensions;


public static class BookMappingExtensions
{
    public static Book ToBook(BookDto bookDto)
    {
        return new Book
        {
            Title = bookDto.Title,
            Author = bookDto.Author,
            Description = bookDto.Description
        };
    }

    public static BookDto ToBookDto(Book book)
    {
        return new BookDto
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            Description = book.Description
        };
    }

}