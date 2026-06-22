namespace Core.Models;

public class Book
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Title { get; set; }
    public required string Author { get; set; }
    public required string Description { get; set; }
    public required decimal Price { get; set; }
}