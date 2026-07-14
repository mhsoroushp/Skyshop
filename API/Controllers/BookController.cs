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
public class BookController(IBookRepository repo) : ControllerBase
{ 
    [HttpGet]
    public async Task<ActionResult<BookPaging>> GetBooks([FromQuery] BookQueryParams bookQueryParams)
    {
        // it is good to replace BookPaging with BookPaging Dto

        var bookPaging = await repo.GetAllAsync(bookQueryParams);
        //var bookDtos = books.Select(book => BookMappingExtensions.ToBookDto(book)).ToList();

        return Ok(bookPaging);
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