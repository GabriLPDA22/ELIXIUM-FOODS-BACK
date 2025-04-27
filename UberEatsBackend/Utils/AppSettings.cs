namespace UberEatsBackend.Utils
{
    public class AppSettings
    {
        public string JwtSecret { get; set; } = string.Empty;
        public string JwtIssuer { get; set; } = string.Empty;
        public string JwtAudience { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;
    }
}
