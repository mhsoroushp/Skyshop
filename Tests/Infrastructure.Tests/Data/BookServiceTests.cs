namespace Infrastructure.Tests.Data;

using Core.Entities;
using Core.Interfaces;
using Core.Queries;
using FluentAssertions;
using Infrastructure.Data;
using Moq;
using Xunit;

public class BookServiceTests
{
    [Fact]
    public async Task GetBookByIdAsync_Should_Return_Null_When_Book_Does_Not_Exist()
    {
        // Arrange
        var repoMock = new Mock<IBookRepository>();
        var blobMock = new Mock<IBlobStorageService>();

        repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Book?)null);

        var service = new BookService(repoMock.Object, blobMock.Object);

        // Act
        var result = await service.GetBookByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
        blobMock.Verify(
            b => b.DownloadImageAsBytesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetBookByIdAsync_Should_Map_Book_And_Encode_Image()
    {
        // Arrange
        var repoMock = new Mock<IBookRepository>();
        var blobMock = new Mock<IBlobStorageService>();
        var id = Guid.NewGuid();
        var book = new Book
        {
            Id = id,
            Title = "Clean Code",
            Author = "Robert C. Martin",
            Description = "Software craftsmanship",
            Price = 40m,
            CoverImageUrl = "https://example.com/image.jpg"
        };
        var bytes = new byte[] { 1, 2, 3, 4 };

        repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(book);
        blobMock.Setup(b => b.DownloadImageAsBytesAsync(book.CoverImageUrl!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bytes);

        var service = new BookService(repoMock.Object, blobMock.Object);

        // Act
        var result = await service.GetBookByIdAsync(id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.Title.Should().Be(book.Title);
        result.Author.Should().Be(book.Author);
        result.Description.Should().Be(book.Description);
        result.Price.Should().Be(book.Price);
        result.CoverImageBase64.Should().Be(Convert.ToBase64String(bytes));
    }

    [Fact]
    public async Task GetBooksAsync_Should_Map_Paging_Metadata_And_Items()
    {
        // Arrange
        var repoMock = new Mock<IBookRepository>();
        var blobMock = new Mock<IBlobStorageService>();
        var query = new BookQueryParams { PageIndex = 1, PageSize = 2 };

        var books = new[]
        {
            new Book
            {
                Id = Guid.NewGuid(),
                Title = "B1",
                Author = "A1",
                Description = "D1",
                Price = 11m,
                CoverImageUrl = "url-1"
            },
            new Book
            {
                Id = Guid.NewGuid(),
                Title = "B2",
                Author = "A2",
                Description = "D2",
                Price = 12m,
                CoverImageUrl = "url-2"
            }
        };

        repoMock.Setup(r => r.GetAllAsync(query))
            .ReturnsAsync(new BookPaging
            {
                HasPreviousPage = true,
                HasNextPage = false,
                TotalPages = 2,
                PageIndex = 1,
                PageSize = 2,
                TotalItems = 4,
                Items = books
            });

        blobMock.Setup(b => b.DownloadImageAsBytesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new byte[] { 7, 8 });

        var service = new BookService(repoMock.Object, blobMock.Object);

        // Act
        var result = await service.GetBooksAsync(query);

        // Assert
        result.TotalItems.Should().Be(4);
        result.TotalPages.Should().Be(2);
        result.PageIndex.Should().Be(1);
        result.PageSize.Should().Be(2);
        result.HasPreviousPage.Should().BeTrue();
        result.HasNextPage.Should().BeFalse();
        result.Items.Should().HaveCount(2);
        result.Items.Select(i => i.Title).Should().ContainInOrder("B1", "B2");

        blobMock.Verify(
            b => b.DownloadImageAsBytesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task GetBooksAsync_Should_Continue_When_Image_Download_Fails()
    {
        // Arrange
        var repoMock = new Mock<IBookRepository>();
        var blobMock = new Mock<IBlobStorageService>();
        var query = new BookQueryParams();
        var book = new Book
        {
            Id = Guid.NewGuid(),
            Title = "B1",
            Author = "A1",
            Description = "D1",
            Price = 11m,
            CoverImageUrl = "broken-url"
        };

        repoMock.Setup(r => r.GetAllAsync(query))
            .ReturnsAsync(new BookPaging
            {
                HasPreviousPage = false,
                HasNextPage = false,
                TotalPages = 1,
                PageIndex = 0,
                PageSize = 5,
                TotalItems = 1,
                Items = new[] { book }
            });

        blobMock.Setup(b => b.DownloadImageAsBytesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("blob unavailable"));

        var service = new BookService(repoMock.Object, blobMock.Object);

        // Act
        var result = await service.GetBooksAsync(query);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().CoverImageBase64.Should().BeEmpty();
    }
}