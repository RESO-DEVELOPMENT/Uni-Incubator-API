using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.User
{
    public class UserResetPasswordDTO
    {
        [Required]
        public string Token { get; set; } = null!;
        [Required]
        [StringLength(64, MinimumLength = 8)]
        public string NewPassword { get; set; } = null!;
    }
}
