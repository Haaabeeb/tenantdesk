public interface IFileStorageService
{
    public Task UploadAsync (Stream file, string fileName);
    public Task DownloadAsync(string storagePath);
    public Task DeleteAsync(string storagePath);
    public Task GetPublicUrlAsync(string storagePath);
    public Task<bool>ExistsAsync(string storagePath);


}