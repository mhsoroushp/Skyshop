using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Data;

public class AzureBlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _containerClient;
    private readonly IHttpClientFactory _httpClientFactory;

    public AzureBlobStorageService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;

        var connectionString = configuration["AzureBlob:ConnectionString"];
        var containerName = configuration["AzureBlob:ContainerName"];

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("AzureBlob:ConnectionString is not configured.");

        if (string.IsNullOrWhiteSpace(containerName))
            throw new InvalidOperationException("AzureBlob:ContainerName is not configured.");

        _containerClient = new BlobContainerClient(connectionString, containerName);
    }

    public async Task<string> UploadImageFromUrlAsync(string imageUrl, string? blobName = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("Image URL is required.", nameof(imageUrl));

        var httpClient = _httpClientFactory.CreateClient();
        using var response = await httpClient.GetAsync(imageUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var imageBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";

        var resolvedBlobName = ResolveBlobName(imageUrl, blobName);
        return await UploadImageBytesAsync(imageBytes, contentType, resolvedBlobName, cancellationToken);
    }

    public async Task<string> UploadImageBytesAsync(byte[] imageBytes, string contentType, string? blobName = null, CancellationToken cancellationToken = default)
    {
        if (imageBytes is null || imageBytes.Length == 0)
            throw new ArgumentException("Image bytes are required.", nameof(imageBytes));

        if (string.IsNullOrWhiteSpace(contentType))
            throw new ArgumentException("Content type is required.", nameof(contentType));

        await EnsureContainerExistsAsync(cancellationToken);

        var resolvedBlobName = !string.IsNullOrWhiteSpace(blobName)
            ? blobName
            : $"images/{Guid.NewGuid():N}{GetFileExtensionFromContentType(contentType)}";

        var blobClient = _containerClient.GetBlobClient(resolvedBlobName);

        await using var stream = new MemoryStream(imageBytes);
        await blobClient.UploadAsync(stream, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        }, cancellationToken);

        return blobClient.Uri.ToString();
    }

    public async Task<byte[]> DownloadImageAsBytesAsync(string imageUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("Image URL is required.", nameof(imageUrl));

        if (IsAzureBlobUrl(imageUrl))
        {
            await EnsureContainerExistsAsync(cancellationToken);

            var blobName = ExtractBlobNameFromUrl(imageUrl);
            var blobClient = _containerClient.GetBlobClient(blobName);

            var downloadResult = await blobClient.DownloadContentAsync(cancellationToken);
            return downloadResult.Value.Content.ToArray();
        }

        var httpClient = _httpClientFactory.CreateClient();
        return await httpClient.GetByteArrayAsync(imageUrl, cancellationToken);
    }

    private async Task EnsureContainerExistsAsync(CancellationToken cancellationToken)
    {
        await _containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);
    }

    private static string ResolveBlobName(string imageUrl, string? blobName)
    {
        if (!string.IsNullOrWhiteSpace(blobName))
            return blobName;

        var extension = GetFileExtension(imageUrl);
        return $"images/{Guid.NewGuid():N}{extension}";
    }

    private static bool IsAzureBlobUrl(string imageUrl)
    {
        if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri))
            return false;

        return uri.Host.Contains("blob.core.windows.net", StringComparison.OrdinalIgnoreCase);
    }

    private static string ExtractBlobNameFromUrl(string imageUrl)
    {
        if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri))
            throw new ArgumentException("Invalid image URL.", nameof(imageUrl));

        var absolutePath = uri.AbsolutePath.Trim('/');
        var slashIndex = absolutePath.IndexOf('/');

        if (slashIndex < 0 || slashIndex == absolutePath.Length - 1)
            throw new ArgumentException("The provided image URL does not contain a blob path.", nameof(imageUrl));

        return absolutePath[(slashIndex + 1)..];
    }

    private static string GetFileExtension(string imageUrl)
    {
        if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri))
            return ".bin";

        var extension = Path.GetExtension(uri.AbsolutePath);
        return string.IsNullOrWhiteSpace(extension) ? ".bin" : extension;
    }

    private static string GetFileExtensionFromContentType(string contentType)
    {
        return contentType.ToLowerInvariant() switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/gif" => ".gif",
            "image/webp" => ".webp",
            "image/bmp" => ".bmp",
            "image/svg+xml" => ".svg",
            _ => ".bin"
        };
    }
}