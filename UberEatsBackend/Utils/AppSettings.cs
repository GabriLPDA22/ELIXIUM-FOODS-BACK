// Utils/AppSettings.cs
namespace UberEatsBackend.Utils
{
    public class AppSettings
    {
        public string JwtSecret { get; set; } = string.Empty;
        public string JwtIssuer { get; set; } = string.Empty;
        public string JwtAudience { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;
        public string? FrontendUrl { get; set; }
        public AWSSettings? AWS { get; set; }
        public SendGridSettings? SendGrid { get; set; } // ← AGREGAR ESTA LÍNEA
    }

    public class AWSSettings
    {
        public string Region { get; set; } = "us-east-1";
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public S3Settings? S3 { get; set; }
        public SESSettings? SES { get; set; }
    }

    public class S3Settings
    {
        public string BucketName { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
    }

    public class SESSettings
    {
        public string FromEmail { get; set; } = "noreply@elixiumfoods.com";
        public string FromName { get; set; } = "Elixium Foods";
        public string Region { get; set; } = "us-east-1";
    }

    public class SendGridSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = "Elixium Foods";
        public string? ReplyToEmail { get; set; }
    }

    public class StorageSettings
    {
        public bool UseS3Storage { get; set; } = true;
    }
}
