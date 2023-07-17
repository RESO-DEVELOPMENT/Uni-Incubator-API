using Microsoft.EntityFrameworkCore;

namespace Application.Domain.Models
{
    [PrimaryKey(nameof(UserId), nameof(Token))]
    public class UserFCMToken
    {
        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;

        public string Token { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTimeHelper.Now();
    }
}