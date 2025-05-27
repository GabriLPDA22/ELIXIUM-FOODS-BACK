// UberEatsBackend/Services/ImageService.cs
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace UberEatsBackend.Services
{
    public class ImageService : IImageService
    {
        private readonly IStorageService _storageService;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };
        private readonly string[] _allowedContentTypes = { 
            "image/jpeg", "image/jpg", "image/png", "image/gif", 
            "image/webp", "image/bmp" 
        };
        private readonly long _maxFileSize = 10 * 1024 * 1024; // 10MB

        public ImageService(IStorageService storageService)
        {
            _storageService = storageService;
        }

        public async Task<ImageUploadResult> UploadImageAsync(IFormFile file, string folder = "general")
        {
            // Validaciones
            ValidateFile(file);

            // Convertir a base64
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var base64Image = Convert.ToBase64String(memoryStream.ToArray());

            // Generar nombre de archivo con prefijo de content type para preservar formato
            var fileName = GenerateFileName(file.FileName, file.ContentType);

            // Subir usando el storage service
            var imageUrl = await _storageService.SaveFileAsync(
                $"data:{file.ContentType};base64,{base64Image}", 
                fileName, 
                folder
            );

            return new ImageUploadResult
            {
                ImageUrl = imageUrl,
                FileName = fileName,
                Size = file.Length,
                ContentType = file.ContentType,
                Folder = folder
            };
        }

        public async Task<ImageUploadResult> UploadImageBase64Async(string base64Image, string fileName, string folder = "general")
        {
            if (string.IsNullOrEmpty(base64Image))
                throw new ArgumentException("Base64 image data cannot be empty");

            // Validar y extraer información del base64
            var processResult = ProcessBase64Image(base64Image);
            var contentType = processResult.contentType;
            var cleanBase64 = processResult.cleanBase64;
            var size = processResult.size;
            
            // Validar content type
            if (!_allowedContentTypes.Contains(contentType))
                throw new ArgumentException($"Invalid image format: {contentType}");

            // Validar tamaño
            if (size > _maxFileSize)
                throw new ArgumentException($"Image too large. Maximum size is {_maxFileSize / 1024 / 1024}MB");

            // Generar nombre de archivo
            var generatedFileName = GenerateFileName(fileName, contentType);

            // Subir imagen
            var imageUrl = await _storageService.SaveFileAsync(base64Image, generatedFileName, folder);

            return new ImageUploadResult
            {
                ImageUrl = imageUrl,
                FileName = generatedFileName,
                Size = size,
                ContentType = contentType,
                Folder = folder
            };
        }

        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return false;

            try
            {
                await _storageService.DeleteFileAsync(imageUrl);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<ImageUploadResult>> UploadMultipleImagesAsync(IFormFileCollection files, string folder = "general")
        {
            var results = new List<ImageUploadResult>();

            foreach (var file in files)
            {
                try
                {
                    var result = await UploadImageAsync(file, folder);
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    // En caso de error con un archivo, continuamos con los otros
                    results.Add(new ImageUploadResult
                    {
                        FileName = file.FileName,
                        ImageUrl = $"ERROR: {ex.Message}",
                        Size = file.Length,
                        ContentType = file.ContentType,
                        Folder = folder
                    });
                }
            }

            return results;
        }

        public async Task<bool> DeleteMultipleImagesAsync(List<string> imageUrls)
        {
            if (imageUrls == null || !imageUrls.Any())
                return false;

            var allDeleted = true;

            foreach (var imageUrl in imageUrls)
            {
                var deleted = await DeleteImageAsync(imageUrl);
                if (!deleted)
                    allDeleted = false;
            }

            return allDeleted;
        }

        private void ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File cannot be null or empty");

            if (file.Length > _maxFileSize)
                throw new ArgumentException($"File too large. Maximum size is {_maxFileSize / 1024 / 1024}MB");

            if (!_allowedContentTypes.Contains(file.ContentType))
                throw new ArgumentException($"Invalid file type: {file.ContentType}");

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!_allowedExtensions.Contains(extension))
                throw new ArgumentException($"Invalid file extension: {extension}");
        }

        private (string contentType, string cleanBase64, long size) ProcessBase64Image(string base64Image)
        {
            string contentType = "image/jpeg"; // default
            string cleanBase64 = base64Image;

            if (base64Image.Contains(","))
            {
                var parts = base64Image.Split(',');
                var header = parts[0]; // data:image/jpeg;base64
                cleanBase64 = parts[1];

                // Extract content type from header
                if (header.Contains("image/"))
                {
                    var typeStart = header.IndexOf("image/");
                    var typeEnd = header.IndexOf(";", typeStart);
                    if (typeEnd > typeStart)
                    {
                        contentType = header.Substring(typeStart, typeEnd - typeStart);
                    }
                }
            }

            // Calculate approximate size
            var base64Length = cleanBase64.Length;
            var size = (long)(base64Length * 0.75); // Base64 is ~33% larger than binary

            return (contentType, cleanBase64, size);
        }

        private string GenerateFileName(string originalFileName, string contentType)
        {
            var extension = GetExtensionFromContentType(contentType);
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName) ?? "image";
            
            // Sanitizar el nombre del archivo - CORREGIDO
            var validChars = nameWithoutExtension
                .Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_')
                .Take(50) // Limitar longitud
                .ToArray(); // Convertir a array
            
            var sanitizedName = new string(validChars); // Crear string del array
            
            if (string.IsNullOrEmpty(sanitizedName))
                sanitizedName = "image";

            return $"{sanitizedName}-{Guid.NewGuid().ToString("N")[..8]}{extension}";
        }

        private string GetExtensionFromContentType(string contentType)
        {
            return contentType switch
            {
                "image/jpeg" => ".jpg",
                "image/jpg" => ".jpg",
                "image/png" => ".png",
                "image/gif" => ".gif",
                "image/webp" => ".webp",
                "image/bmp" => ".bmp",
                "image/tiff" => ".tiff",
                _ => ".jpg" // default
            };
        }
    }
}