using System.ComponentModel.DataAnnotations;

public interface IFileStorageService
{
    public Task UploadAsync (Stream file, string fileName);
    public Task <Stream> DownloadAsync(string storagePath);
    public Task DeleteAsync(string storagePath);
    public Task <String>GetPublicUrlAsync(string storagePath);
    public Task<bool>ExistsAsync(string storagePath);


}