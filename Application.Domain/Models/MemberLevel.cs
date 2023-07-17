using Microsoft.EntityFrameworkCore;

namespace Application.Domain.Models
{
    [PrimaryKey(nameof(LevelId), nameof(MemberId))]
    public class MemberLevel
    {
        public int LevelId { get; set; }
        public virtual Level Level { get; set; } = null!;

        public Guid MemberId { get; set; }
        public virtual Member Member { get; set; } = null!;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTimeHelper.Now();
    }
}