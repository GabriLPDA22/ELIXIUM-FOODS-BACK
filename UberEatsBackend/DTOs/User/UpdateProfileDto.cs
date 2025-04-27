using System;
using System.Collections.Generic;

namespace UberEatsBackend.DTOs.User
{
    public class UpdateProfileDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Birthdate { get; set; }
        public string? Bio { get; set; }
        public List<string>? DietaryPreferences { get; set; }
        public string? PhotoURL { get; set; }
    }
}
