// UberEatsBackend/Services/IImageService.cs
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UberEatsBackend.Services
{
    public interface IImageService
    {
        Task<ImageUploadResult> UploadImageAsync(IFormFile file, string folder = "general");
        Task<ImageUploadResult> UploadImageBase64Async(string base64Image, string fileName, string folder = "general");
        Task<bool> DeleteImageAsync(string imageUrl);
        Task<List<ImageUploadResult>> UploadMultipleImagesAsync(IFormFileCollection files, string folder = "general");
        Task<bool> DeleteMultipleImagesAsync(List<string> imageUrls);
    }

    public class ImageUploadResult
    {
        public string ImageUrl { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long Size { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public string Folder { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}