using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.User
{
    public class UserCreateDTO
    {
        [EmailAddress]
        public string EmailAddress { get; set; } = null!;
        [StringLength(64, MinimumLength = 8)]
        public string Password { get; set; } = null!;

        [Required]
        [MinLength(4)]
        public string FullName { get; set; } = null!;
        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = null!;
        public bool SendEmail { get; set; } = false;

        public bool IsAdmin { get; set; } = true;
    }
}