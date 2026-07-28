using API.DTOs;
using Core.Queries;
using API.Extensions;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Core.Models;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
public class BookController(IBookRepository repo, IBlobStorageService blobStorageService) : ControllerBase
{ 
    [HttpGet]
    public async Task<ActionResult<BookPagingDto>> GetBooks([FromQuery] BookQueryParams bookQueryParams)
    {
        // it is good to replace BookPaging with BookPaging Dto

        var bookPaging = await repo.GetAllAsync(bookQueryParams);
        //var bookDtos = books.Select(book => BookMappingExtensions.ToBookDto(book)).ToList();

        var bookPagingDto = new BookPagingDto
        {
            HasPreviousPage = bookPaging.HasPreviousPage,
            HasNextPage = bookPaging.HasNextPage,
            TotalPages = bookPaging.TotalPages,
            PageIndex = bookPaging.PageIndex,
            PageSize = bookPaging.PageSize,
            TotalItems = bookPaging.TotalItems,
            Items = bookPaging.Items.Select(book => BookMappingExtensions.ToBookDto(book)).ToList()
        };

        foreach (var book in bookPagingDto.Items)
        {
            try
            {
                var imageBytes = await blobStorageService.DownloadImageAsBytesAsync(book.CoverImageUrl ?? string.Empty);
                book.CoverImageBase64 = Convert.ToBase64String(imageBytes);
            }
            catch (Exception ex)
            {
                // Log the exception and continue
                Console.WriteLine($"Error downloading image for book {book.Id}: {ex.Message}");
                book.CoverImageBase64 = string.Empty; // or set to a default image
            }
        }

        return Ok(bookPagingDto);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BookDto>> GetBook(string id)
    {
        if(!Guid.TryParse(id, out var guid))
        {
            return BadRequest(new {message="invalid guid"});
        }

        var book = await repo.GetByIdAsync(guid);
        if (book == null)
        {
            return NotFound(new {message = $"Book with id {id} not found."});
        }

        var bookDto = BookMappingExtensions.ToBookDto(book);

        try
        {
            var imageBytes = await blobStorageService.DownloadImageAsBytesAsync(bookDto.CoverImageUrl ?? string.Empty);
            bookDto.CoverImageBase64 = Convert.ToBase64String(imageBytes);
        }
        catch (Exception ex)
        {
            // Log the exception and continue
            Console.WriteLine($"Error downloading image for book {bookDto.Id}: {ex.Message}");
            bookDto.CoverImageBase64 = string.Empty; // or set to a default image
        }

        return Ok(bookDto);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBook(BookDto bookDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new {message = "Book data is required."});
        }

        var book = BookMappingExtensions.ToBook(bookDto);

        await repo.AddAsync(book); 

        // TODO should we return book or bookDto? 

        if (await repo.SaveChangesAsync())
        {
            var bookDtoResponse = BookMappingExtensions.ToBookDto(book);

            return CreatedAtAction(nameof(GetBook), new { id = book.Id }, bookDtoResponse);
        }
        return BadRequest(new {message = "Failed to create book."});
    }

    [HttpPut]
    public async Task<ActionResult> UpdateBook([FromBody] BookDto bookDto)
    {
        if(!ModelState.IsValid)
        {
            return BadRequest(new {message = "Missing data"});
        }

        var book = await repo.GetByIdAsync(bookDto.Id);
        if(book == null)
        {
            return NotFound(new {message = "the resource not exist"});
        }

        book.Title = bookDto.Title;
        book.Author = bookDto.Author;
        book.Description = bookDto.Description;

        repo.UpdateAsync(book);

        if(!await repo.SaveChangesAsync())
        {
            return BadRequest(new {message = "Failed to update book."});
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBook(string id)
    {
        if(!Guid.TryParse(id, out var guid))
        {
            return BadRequest(new {message="invalid guid"});
        }

        var book = await repo.GetByIdAsync(guid);
        if (book == null)
        {
            return NotFound(new {message = $"Book with id {id} not found."});
        }
        await repo.DeleteAsync(guid);
        if(await repo.SaveChangesAsync())
        {
            return Ok("is deleted successfully");
        }
        return NoContent();
    }
}