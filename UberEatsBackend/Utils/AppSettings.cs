// UberEatsBackend/Utils/AppSettings.cs
namespace UberEatsBackend.Utils
{
    public class AppSettings
    {
        public string JwtSecret { get; set; } = string.Empty;
        public string JwtIssuer { get; set; } = string.Empty;
        public string JwtAudience { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;
        public AWSSettings AWS { get; set; } = new AWSSettings();
        public StorageSettings StorageSettings { get; set; } = new StorageSettings();
    }

    public class AWSSettings
    {
        public string Region { get; set; } = string.Empty;
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string SessionToken { get; set; } = string.Empty;
        public S3Settings S3 { get; set; } = new S3Settings();
    }

    public class S3Settings
    {
        public string BucketName { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
    }

    public class StorageSettings
    {
        public bool UseS3Storage { get; set; } = false;
    }
}