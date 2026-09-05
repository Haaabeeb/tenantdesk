using TenantDesk.Application.Interfaces.Services;

namespace TenantDesk.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _rootPath;

    public LocalFileStorageService()
    {
        // Files are stored under a local folder at the solution root: /App_Data/uploads
        _rootPath = Path.Combine(AppContext.BaseDirectory, "App_Data", "uploads");
        Directory.CreateDirectory(_rootPath);
    }

    public async Task UploadAsync(Stream file, string fileName)
    {
        var fullPath = GetFullPath(fileName);

        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        await using var outputStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
        await file.CopyToAsync(outputStream);
    }

    public Task<Stream> DownloadAsync(string storagePath)
    {
        var fullPath = GetFullPath(storagePath);

        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"File not found at '{storagePath}'.");

        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storagePath)
    {
        var fullPath = GetFullPath(storagePath);

        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }

    public Task<string> GetPublicUrlAsync(string storagePath)
    {
        // No real public hosting for local disk storage — returning a relative API path
        // that a future "download" endpoint could serve from.
        return Task.FromResult($"/api/attachments/files/{Uri.EscapeDataString(storagePath)}");
    }

    public Task<bool> ExistsAsync(string storagePath)
    {
        var fullPath = GetFullPath(storagePath);
        return Task.FromResult(File.Exists(fullPath));
    }

    private string GetFullPath(string storagePath) =>
        Path.Combine(_rootPath, storagePath);
}