using Application.Domain.Enums.ProjectSalaryCycleRequest;
using Application.Helpers;

namespace Application.QueryParams.ProjectSalaryCycleRequest
{
    public class ProjectsSalaryCycleRequestQueryParams : PaginationParams
    {
        public Guid? ProjectId { get; set; }
        public ProjectSalaryCycleRequestOrderBy OrderBy { get; set; } = ProjectSalaryCycleRequestOrderBy.DateDesc;
        public List<ProjectSalaryCycleRequestStatus> Status { get; set; } = new List<ProjectSalaryCycleRequestStatus>();
    }
}