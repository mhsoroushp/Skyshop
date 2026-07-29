using API.DTOs;
using Core.Queries;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
public class BookController(IBookService _bookService) : ControllerBase
{ 
    [HttpGet]
    public async Task<ActionResult<BookPagingDto>> GetBooks([FromQuery] BookQueryParams bookQueryParams)
    {

        var bookPagingDto = await _bookService.GetBooksAsync(bookQueryParams);
        return Ok(bookPagingDto);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BookDto>> GetBook(string id)
    {
        if(!Guid.TryParse(id, out var guid))
        {
            return BadRequest(new {message="invalid guid"});
        }

        var bookDto = await _bookService.GetBookByIdAsync(guid);

        return Ok(bookDto);
    }
}