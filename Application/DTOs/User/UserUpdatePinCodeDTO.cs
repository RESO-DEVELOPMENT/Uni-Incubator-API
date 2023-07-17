using System.ComponentModel.DataAnnotations;
using Application.Domain.Enums.User;

namespace Application.DTOs.User
{
    public class UserUpdatePinCodeDTO
    {
        public string? OldPinCode { get; set; }

        [StringLength(6, MinimumLength = 6)]
        [Required]
        public string NewPinCode { get; set; }
    }
}