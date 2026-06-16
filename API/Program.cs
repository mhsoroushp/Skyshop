using Core;
using Core.Interfaces;
using Core.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<BookContext>(opt =>
    opt.UseInMemoryDatabase("BookDb"));

builder.Services.AddScoped<IBookRepository, BookRepository>(); 
builder.Services.AddOpenApiDocument();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    // app.UseSwaggerUi(options =>
    // {
    //     options.DocumentPath = "/openapi/v1.json";
    // }); 
    app.UseSwaggerUi();

}

app.UseCors(x => x
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()
    .WithOrigins("http://localhost:4200", "https://localhost:4200"));

app.UseHttpsRedirection();

app.UseAuthorization();

// Seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BookContext>();

    // Add some data
    context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "First Book", Author = "Author 1", Description = "Description 1" });
    context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Second Book", Author = "Author 2", Description = "Description 2" });
    context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Third Book", Author = "Author 3", Description = "Description 3" });
    context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Fourth Book", Author = "Author 4", Description = "Description 4" });
    context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Fifth Book", Author = "Author 5", Description = "Description 5" });
    context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Sixth Book", Author = "Author 6", Description = "Description 6" });
    context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Seventh Book", Author = "Author 7", Description = "Description 7" });
    context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Eighth Book", Author = "Author 8", Description = "Description 8" });
    context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Ninth Book", Author = "Author 9", Description = "Description 9" });
    context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Tenth Book", Author = "Author 10", Description = "Description 10" });
    context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Eleventh Book", Author = "Author 11", Description = "Description 11" });
    context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Twelfth Book", Author = "Author 12", Description = "Description 12" });
    context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Thirteenth Book", Author = "Author 13", Description = "Description 13" });
    context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Fourteenth Book", Author = "Author 14", Description = "Description 14" });
    context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Fifteenth Book", Author = "Author 15", Description = "Description 15" });
    context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Sixteenth Book", Author = "Author 16", Description = "Description 16" });
    context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Seventeenth Book", Author = "Author 17", Description = "Description 17" });
    context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Eighteenth Book", Author = "Author 18", Description = "Description 18" });
    context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Nineteenth Book", Author = "Author 19", Description = "Description 19" });
    context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Twentieth Book", Author = "Author 20", Description = "Description 20" });
    context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Twenty-first Book", Author = "Author 21", Description = "Description 21" });
    context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Twenty-second Book", Author = "Author 22", Description = "Description 22" });
    context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Twenty-third Book", Author = "Author 23", Description = "Description 23" });
    context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Twenty-fourth Book", Author = "Author 24", Description = "Description 24" });
    context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Twenty-fifth Book", Author = "Author 25", Description = "Description 25" });
    context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Twenty-sixth Book", Author = "Author 26", Description = "Description 26" });
    context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Twenty-seventh Book", Author = "Author 27", Description = "Description 27" });
    context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Twenty-eighth Book", Author = "Author 28", Description = "Description 28" });
    context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Twenty-ninth Book", Author = "Author 29", Description = "Description 29" });
    context.Books.Add(new Book { Id = Guid.NewGuid(), Title = "Thirtieth Book", Author = "Author 30", Description = "Description 30" });
    context.SaveChanges();
}

app.MapControllers();

app.Run();
