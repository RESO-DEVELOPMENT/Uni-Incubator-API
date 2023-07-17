using Application.Domain.Enums.ProjectMemberRequest;
using Application.Domain.Enums.ProjectSalaryCycleRequest;
using Application.DTOs.Member;
using Application.DTOs.ProjectSalaryCycle;

namespace Application.DTOs.ProjectSalaryCycleRequest
{
    public class ProjectSalaryCycleRequestWithSalaryCycleDTO
    {
        public Guid RequestId { get; set; }

        public virtual MemberDTO Member { get; set; } = null!;
        public virtual ProjectSalaryCycleDTO ProjectSalaryCycle { get; set; } = null!;

        public ProjectSalaryCycleRequestType Type { get; set; }
        public string Value { get; set; } = null!;

        public ProjectMemberRequestStatus Status { get; set; } = ProjectMemberRequestStatus.Created;
        public DateTime? ReviewedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}