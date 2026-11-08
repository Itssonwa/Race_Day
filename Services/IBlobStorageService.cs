namespace RaceDay.Services
{
   public record BlobUploudResult(string BlobName, string Url);
    public interface IBlobStorageService
    {
        String? Validate(IFormFile file);
        Task<BlobUploudResult> UploadBannerAsync(IFormFile file, CancellationToken ct = default);
        Task DeleteAsync(string? blobName,  CancellationToken ct = default);
    }
}
