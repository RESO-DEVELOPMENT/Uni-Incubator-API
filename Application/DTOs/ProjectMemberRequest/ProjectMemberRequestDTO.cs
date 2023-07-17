using Application.Domain.Enums.ProjectMemberRequest;
using Application.DTOs.Member;

namespace Application.DTOs.ProjectMemberRequest
{
    public class ProjectMemberRequestDTO
    {
        public Guid RequestId { get; set; }

        public MemberWithLevelDTO Member { get; set; } = null!;

        public string Major { get; set; } = null!;
        public string Note { get; set; } = null!;

        public ProjectMemberRequestStatus Status { get; set; }

        public DateTime? ReviewedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}