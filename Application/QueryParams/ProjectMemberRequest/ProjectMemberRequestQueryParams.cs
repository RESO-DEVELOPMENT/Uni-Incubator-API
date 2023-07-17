using Application.Domain.Enums.ProjectMemberRequest;
using Application.Helpers;

namespace Application.QueryParams.ProjectMemberRequest
{
    public class ProjectMemberRequestQueryParams : PaginationParams
    {
        public string? MemberEmail { get; set; }
        public List<ProjectMemberRequestStatus> Status { get; set; } = new List<ProjectMemberRequestStatus>();
    }
}