// UberEatsBackend/Services/S3StorageService.cs
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace UberEatsBackend.Services
{
    public class S3StorageService : IStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly ILogger<S3StorageService> _logger;
        private readonly string _bucketName;
        private readonly string _baseUrl;

        public S3StorageService(IAmazonS3 s3Client, IConfiguration configuration, ILogger<S3StorageService> logger)
        {
            _s3Client = s3Client;
            _logger = logger;
            
            // Obtener configuración directamente desde IConfiguration
            _bucketName = configuration["AWS:S3:BucketName"] ?? throw new InvalidOperationException("AWS:S3:BucketName no configurado");
            _baseUrl = configuration["AWS:S3:BaseUrl"] ?? throw new InvalidOperationException("AWS:S3:BaseUrl no configurado");
            
            _logger.LogInformation($"S3StorageService initialized - Bucket: {_bucketName}, BaseUrl: {_baseUrl}");
        }

        public async Task<string> SaveFileAsync(string base64File, string fileName, string folder = "general")
        {
            if (string.IsNullOrEmpty(base64File))
            {
                _logger.LogWarning("Empty base64 file provided");
                return string.Empty;
            }

            try
            {
                _logger.LogInformation($"Starting S3 upload - File: {fileName}, Folder: {folder}, Bucket: {_bucketName}");

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

                _logger.LogInformation($"Detected content type: {contentType}");

                // Convert base64 to bytes
                byte[] fileBytes;
                try
                {
                    fileBytes = Convert.FromBase64String(base64Data);
                    _logger.LogInformation($"Successfully converted base64 to bytes: {fileBytes.Length} bytes");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to convert base64 to bytes");
                    throw new Exception("Invalid base64 data", ex);
                }

                // Generate unique file name with proper extension and folder
                var extension = GetExtensionFromContentType(contentType);
                var uniqueFileName = $"{folder}/{Guid.NewGuid()}-{Path.GetFileNameWithoutExtension(fileName)}{extension}";
                
                _logger.LogInformation($"Generated S3 key: {uniqueFileName}");

                // Verify bucket name before making request
                if (string.IsNullOrEmpty(_bucketName))
                {
                    throw new InvalidOperationException("S3 Bucket name is not configured");
                }

                // Test S3 connection first
                try
                {
                    await _s3Client.ListBucketsAsync();
                    _logger.LogInformation("S3 connection test successful");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "S3 connection test failed");
                    throw new Exception("Failed to connect to S3", ex);
                }

                // Create the request
                var request = new PutObjectRequest
                {
                    BucketName = _bucketName, // Usar la variable de instancia
                    Key = uniqueFileName,
                    InputStream = new MemoryStream(fileBytes),
                    ContentType = contentType
                    // No usar CannedACL porque el bucket no permite ACLs
                };

                // Add metadata
                request.Metadata.Add("uploaded-by", "ubereats-backend");
                request.Metadata.Add("upload-date", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));

                _logger.LogInformation($"Uploading to S3 bucket: {_bucketName} with key: {uniqueFileName}");

                // Upload to S3
                var response = await _s3Client.PutObjectAsync(request);
                
                _logger.LogInformation($"S3 response status: {response.HttpStatusCode}");

                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    // Return the public URL
                    var publicUrl = $"{_baseUrl.TrimEnd('/')}/{uniqueFileName}";
                    _logger.LogInformation($"File uploaded successfully: {publicUrl}");
                    return publicUrl;
                }
                else
                {
                    _logger.LogError($"S3 upload failed with status: {response.HttpStatusCode}");
                    throw new Exception($"Failed to upload file to S3. Status: {response.HttpStatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading file to S3: {ex.Message}");
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
            {
                _logger.LogWarning("Empty file URL provided for deletion");
                return;
            }

            try
            {
                _logger.LogInformation($"Attempting to delete file: {fileUrl}");
                
                // Extract the key from the URL
                var key = ExtractKeyFromUrl(fileUrl);
                if (string.IsNullOrEmpty(key))
                {
                    _logger.LogWarning($"Could not extract key from URL: {fileUrl}");
                    return;
                }

                _logger.LogInformation($"Deleting S3 object with key: {key}");

                var request = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };

                await _s3Client.DeleteObjectAsync(request);
                _logger.LogInformation($"Successfully deleted file: {key}");
            }
            catch (Exception ex)
            {
                // Log the error but don't throw - deletion failures shouldn't break the application
                _logger.LogError(ex, $"Error deleting file from S3: {fileUrl}");
            }
        }

        private string GetExtensionFromContentType(string contentType)
        {
            var extension = contentType switch
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
            
            _logger.LogDebug($"Content type {contentType} mapped to extension {extension}");
            return extension;
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
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error extracting key from URL: {fileUrl}");
                return null;
            }
        }
    }
}