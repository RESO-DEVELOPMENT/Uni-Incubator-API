using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.User
{
    public class LoginDTO
    {
        [Required]
        public string EmailAddress { get; set; } = null!;
        [Required]
        public string Password { get; set; } = null!;
    }
}