// UberEatsBackend/Services/LocalStorageService.cs - ACTUALIZADO
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using UberEatsBackend.Utils;

namespace UberEatsBackend.Services
{
    public class LocalStorageService : IStorageService
    {
        private readonly AppSettings _appSettings;
        private readonly string _uploadsFolder;

        public LocalStorageService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
            _uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            // Ensure the uploads directory exists
            if (!Directory.Exists(_uploadsFolder))
            {
                Directory.CreateDirectory(_uploadsFolder);
            }
        }

        public async Task<string> SaveFileAsync(string base64File, string fileName, string folder = "general")
        {
            if (string.IsNullOrEmpty(base64File))
                return string.Empty;

            // Create folder if it doesn't exist
            var folderPath = Path.Combine(_uploadsFolder, folder);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Remove the data:image/xyz;base64, part
            string base64Data = base64File;
            if (base64File.Contains(","))
            {
                base64Data = base64File.Split(',')[1];
            }

            // Generate a unique filename
            string uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            string filePath = Path.Combine(folderPath, uniqueFileName);

            // Convert base64 to bytes and save
            byte[] fileBytes = Convert.FromBase64String(base64Data);
            await File.WriteAllBytesAsync(filePath, fileBytes);

            // Return URL path for accessing the file
            return $"/uploads/{folder}/{uniqueFileName}";
        }

        // Backward compatibility method
        public async Task<string> SaveFileAsync(string base64File, string fileName)
        {
            return await SaveFileAsync(base64File, fileName, "general");
        }

        public async Task DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl))
                return;

            try
            {
                // Extract relative path from URL
                var relativePath = fileUrl.TrimStart('/');
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);

                // Delete the file if it exists
                if (File.Exists(filePath))
                {
                    await Task.Run(() => File.Delete(filePath));
                }
            }
            catch (Exception ex)
            {
                // Log error but don't throw
                Console.WriteLine($"Error deleting local file: {ex.Message}");
            }
        }
    }
}