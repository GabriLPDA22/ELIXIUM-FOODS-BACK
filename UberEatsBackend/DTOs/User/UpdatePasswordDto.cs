namespace UberEatsBackend.DTOs.User
{
    public class UpdatePasswordDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
