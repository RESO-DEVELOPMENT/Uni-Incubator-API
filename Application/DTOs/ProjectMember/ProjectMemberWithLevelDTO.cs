using Application.Domain.Enums.ProjectMember;
using Application.DTOs.Member;

namespace Application.DTOs.ProjectMember
{
    public class ProjectMemberWithLevelDTO
    {
        // public Guid ProjectId { get; set; }
        public Guid ProjectMemberId { get; set; }
        public MemberWithLevelDTO Member { get; set; } = null!;
        public ProjectMemberRole Role { get; set; }
        public string Major { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        // public ProjectMemberStatus Status { get; set; }

    }
}