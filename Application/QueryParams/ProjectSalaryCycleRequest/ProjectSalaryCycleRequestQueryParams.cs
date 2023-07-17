using Application.Domain.Enums.ProjectSalaryCycleRequest;
using Application.Helpers;

namespace Application.QueryParams.ProjectSalaryCycleRequest
{
    public class ProjectSalaryCycleRequestQueryParams : PaginationParams
    {
        public ProjectSalaryCycleRequestOrderBy OrderBy { get; set; } = ProjectSalaryCycleRequestOrderBy.DateDesc;
        public List<ProjectSalaryCycleRequestStatus> Status { get; set; } = new List<ProjectSalaryCycleRequestStatus>();
    }
}