using System.ComponentModel.DataAnnotations;

namespace Application.Domain.Models
{
    public class Role
    {
        [Key]
        public string RoleId { get; set; } = null!;
        public string RoleName { get; set; } = null!;

        public virtual List<User> Users { get; set; } = new List<User>();
    }
}