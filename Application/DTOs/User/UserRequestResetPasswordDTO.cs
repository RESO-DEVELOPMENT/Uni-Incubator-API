using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.User
{
    public class UserRequestResetPasswordDTO
    {
        [Required]
        public string EmailAddress { get; set; } = null!;
    }
}
