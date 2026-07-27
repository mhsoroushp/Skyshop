namespace Core.Interfaces;

public interface IBlobStorageService
{
    Task<string> UploadImageBytesAsync(byte[] imageBytes, string contentType, string? blobName = null, CancellationToken cancellationToken = default);
    Task<string> UploadImageFromUrlAsync(string imageUrl, string? blobName = null, CancellationToken cancellationToken = default);
    Task<byte[]> DownloadImageAsBytesAsync(string imageUrl, CancellationToken cancellationToken = default);
}