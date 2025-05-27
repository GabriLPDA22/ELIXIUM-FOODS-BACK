// UberEatsBackend/Services/S3StorageService.cs
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;
using UberEatsBackend.Utils;

namespace UberEatsBackend.Services
{
    public class S3StorageService : IStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly AppSettings _appSettings;
        private readonly string _bucketName;
        private readonly string _baseUrl;

        public S3StorageService(IAmazonS3 s3Client, IOptions<AppSettings> appSettings)
        {
            _s3Client = s3Client;
            _appSettings = appSettings.Value;
            _bucketName = _appSettings.AWS.S3.BucketName;
            _baseUrl = _appSettings.AWS.S3.BaseUrl;
        }

        public async Task<string> SaveFileAsync(string base64File, string fileName, string folder = "general")
        {
            if (string.IsNullOrEmpty(base64File))
                return string.Empty;

            try
            {
                // Remove the data:image/xyz;base64, part if present
                string base64Data = base64File;
                string contentType = "image/jpeg"; // default

                if (base64File.Contains(","))
                {
                    var parts = base64File.Split(',');
                    var header = parts[0]; // data:image/jpeg;base64
                    base64Data = parts[1];

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

                // Convert base64 to bytes
                byte[] fileBytes = Convert.FromBase64String(base64Data);

                // Generate unique file name with proper extension and folder
                var extension = GetExtensionFromContentType(contentType);
                var uniqueFileName = $"{folder}/{Guid.NewGuid()}-{Path.GetFileNameWithoutExtension(fileName)}{extension}";

                // Create the request
                var request = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = uniqueFileName,
                    InputStream = new MemoryStream(fileBytes),
                    ContentType = contentType,
                    CannedACL = S3CannedACL.PublicRead // Make the file publicly accessible
                };

                // Add metadata
                request.Metadata.Add("uploaded-by", "ubereats-backend");
                request.Metadata.Add("upload-date", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));

                // Upload to S3
                var response = await _s3Client.PutObjectAsync(request);

                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    // Return the public URL
                    return $"{_baseUrl.TrimEnd('/')}/{uniqueFileName}";
                }
                else
                {
                    throw new Exception($"Failed to upload file to S3. Status: {response.HttpStatusCode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error uploading file to S3: {ex.Message}", ex);
            }
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
                // Extract the key from the URL
                var key = ExtractKeyFromUrl(fileUrl);
                if (string.IsNullOrEmpty(key))
                    return;

                var request = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };

                await _s3Client.DeleteObjectAsync(request);
            }
            catch (Exception ex)
            {
                // Log the error but don't throw - deletion failures shouldn't break the application
                Console.WriteLine($"Error deleting file from S3: {ex.Message}");
            }
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

        private string ExtractKeyFromUrl(string fileUrl)
        {
            try
            {
                if (fileUrl.StartsWith(_baseUrl))
                {
                    return fileUrl.Substring(_baseUrl.Length).TrimStart('/');
                }

                // Try to extract from other possible S3 URL formats
                var uri = new Uri(fileUrl);
                if (uri.Host.Contains("amazonaws.com"))
                {
                    return uri.AbsolutePath.TrimStart('/');
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}