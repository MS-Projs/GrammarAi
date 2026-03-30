using Amazon.S3;
using Amazon.S3.Model;
using GrammarAi.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace GrammarAi.Infrastructure.Services;

public class S3StorageService(IAmazonS3 s3, IConfiguration config) : IStorageService
{
    private readonly string _bucket = config["Storage:BucketName"]!;
    private readonly string _publicBaseUrl = config["Storage:PublicBaseUrl"] ?? string.Empty;

    public async Task<(string Key, string Url)> UploadAsync(Stream stream, string fileName, string contentType, CancellationToken ct = default)
    {
        var key = fileName; // caller provides the full key path
        var request = new PutObjectRequest
        {
            BucketName = _bucket,
            Key = key,
            InputStream = stream,
            ContentType = contentType,
            DisablePayloadSigning = true
        };

        await s3.PutObjectAsync(request, ct);
        var url = string.IsNullOrEmpty(_publicBaseUrl)
            ? $"https://{_bucket}.s3.amazonaws.com/{key}"
            : $"{_publicBaseUrl.TrimEnd('/')}/{key}";

        return (key, url);
    }

    public async Task<byte[]> DownloadAsync(string key, CancellationToken ct = default)
    {
        var request = new GetObjectRequest { BucketName = _bucket, Key = key };
        using var response = await s3.GetObjectAsync(request, ct);
        using var ms = new MemoryStream();
        await response.ResponseStream.CopyToAsync(ms, ct);
        return ms.ToArray();
    }

    public async Task DeleteAsync(string key, CancellationToken ct = default)
    {
        await s3.DeleteObjectAsync(_bucket, key, ct);
    }
}
