using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.User
{
    public class UserChangePasswordDTO
    {
        [Required]
        public string OldPassword { get; set; } = null!;
        [Required]
        [StringLength(64, MinimumLength = 8)]
        public string NewPassword { get; set; } = null!;
    }
}
