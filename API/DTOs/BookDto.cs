
using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class BookDto
{
    public Guid Id { get; set; }
    [Required]
    public string Title { get; set; } = string.Empty;
    [Required]  
    public string Author { get; set; } = string.Empty;
    [Required]
    public string Description { get; set; } = string.Empty;
}

