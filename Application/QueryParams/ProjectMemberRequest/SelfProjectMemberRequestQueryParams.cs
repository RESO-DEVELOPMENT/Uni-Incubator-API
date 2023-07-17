using Application.Domain.Enums.ProjectMemberRequest;
using Application.Helpers;

namespace Application.QueryParams.ProjectMemberRequest
{
    public class SelfProjectMemberRequestQueryParams : PaginationParams
    {
        public Guid? ProjectId { get; set; }
        public List<ProjectMemberRequestStatus> Status { get; set; } = new List<ProjectMemberRequestStatus>();
    }
}