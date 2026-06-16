using Core.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class TodoController : ControllerBase
{
    private readonly IBookRepository repository;

    public TodoController(IBookRepository repository)
    {
        this.repository = repository;
    }

    // [HttpGet(Name = "GetTodoItems")]
    // public async Task<ActionResult<IReadOnlyList<TodoItem>>> Get()
    // {
    //     var todoItems = await repository.GetAllAsync();
    //     return Ok(todoItems);
    // }
}
