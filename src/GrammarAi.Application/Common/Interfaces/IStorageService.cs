namespace GrammarAi.Application.Common.Interfaces;

public interface IStorageService
{
    Task<(string Key, string Url)> UploadAsync(Stream stream, string fileName, string contentType, CancellationToken ct = default);
    Task<byte[]> DownloadAsync(string key, CancellationToken ct = default);
    Task DeleteAsync(string key, CancellationToken ct = default);
}
