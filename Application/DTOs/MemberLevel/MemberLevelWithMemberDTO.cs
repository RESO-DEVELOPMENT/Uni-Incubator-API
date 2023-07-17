using Application.DTOs.Level;
using Application.DTOs.Member;

namespace Application.DTOs.MemberLevel
{
    public class MemberLevelWithMemberDTO
    {
        public LevelDTO Level { get; set; } = null!;
        public MemberDTO Member { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}