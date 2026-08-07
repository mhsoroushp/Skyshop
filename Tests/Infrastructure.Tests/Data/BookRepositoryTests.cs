namespace Infrastructure.Tests.Data;

using Core.Entities;
using Core.Queries;
using FluentAssertions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class BookRepositoryTests
{
    private static BookContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BookContext>()
            .UseInMemoryDatabase(databaseName: $"book-repo-tests-{Guid.NewGuid()}")
            .Options;

        return new BookContext(options);
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_Filtered_And_Paged_Data()
    {
        // Arrange
        using var context = CreateContext();
        var books = new List<Book>
        {
            new() { Title = "B1", Author = "john", Description = "d", Price = 10m },
            new() { Title = "B2", Author = "john", Description = "d", Price = 11m },
            new() { Title = "B3", Author = "john", Description = "d", Price = 12m },
            new() { Title = "B4", Author = "mary", Description = "d", Price = 13m }
        };

        await context.Books.AddRangeAsync(books);
        await context.SaveChangesAsync();

        var repository = new BookRepository(context);
        var query = new BookQueryParams
        {
            SearchText = "john",
            PageIndex = 0,
            PageSize = 2
        };

        // Act
        var result = await repository.GetAllAsync(query);

        // Assert
        result.TotalItems.Should().Be(3);
        result.TotalPages.Should().Be(2);
        result.PageIndex.Should().Be(0);
        result.PageSize.Should().Be(2);
        result.HasPreviousPage.Should().BeFalse();
        result.HasNextPage.Should().BeTrue();
        result.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(b => b.Author == "john");
    }

    [Fact]
    public async Task DeleteAsync_Should_Remove_Book_When_It_Exists()
    {
        // Arrange
        using var context = CreateContext();
        var existing = new Book
        {
            Title = "ToDelete",
            Author = "author",
            Description = "desc",
            Price = 9m
        };

        await context.Books.AddAsync(existing);
        await context.SaveChangesAsync();

        var repository = new BookRepository(context);

        // Act
        await repository.DeleteAsync(existing.Id);
        var saved = await repository.SaveChangesAsync();

        // Assert
        saved.Should().BeTrue();
        var fromDb = await context.Books.FindAsync(existing.Id);
        fromDb.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_Should_Persist_Modified_Fields()
    {
        // Arrange
        using var context = CreateContext();
        var existing = new Book
        {
            Title = "Old title",
            Author = "author",
            Description = "desc",
            Price = 9m
        };

        await context.Books.AddAsync(existing);
        await context.SaveChangesAsync();

        var repository = new BookRepository(context);
        existing.Title = "Updated title";

        // Act
        repository.UpdateAsync(existing);
        await repository.SaveChangesAsync();

        // Assert
        var updated = await repository.GetByIdAsync(existing.Id);
        updated.Should().NotBeNull();
        updated!.Title.Should().Be("Updated title");
    }
}
