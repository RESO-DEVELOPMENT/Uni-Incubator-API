using Application.Domain.Enums.ProjectMember;
using Application.DTOs.Member;

namespace Application.DTOs.ProjectMember
{
    public class ProjectMemberDTO
    {
        public Guid ProjectMemberId { get; set; }
        public MemberDTO Member { get; set; } = null!;
        public ProjectMemberRole Role { get; set; }

        public String Major { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        // public ProjectMemberStatus Status { get; set; }

    }
}