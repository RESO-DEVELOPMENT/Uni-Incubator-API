using System.ComponentModel.DataAnnotations;
using Application.Domain.Enums.User;

namespace Application.DTOs.User
{
    public class UserUpdateDTO
    {
        [Required]
        public Guid UserId { get; set; }

        public UserStatus? Status { get; set; }
    }
}