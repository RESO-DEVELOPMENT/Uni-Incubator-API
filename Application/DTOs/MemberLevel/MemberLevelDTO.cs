using Application.DTOs.Level;

namespace Application.DTOs.MemberLevel
{
    public class MemberLevelDTO
    {
        public LevelDTO Level { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}