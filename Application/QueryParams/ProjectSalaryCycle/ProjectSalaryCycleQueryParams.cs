using Application.Domain.Enums.ProjectSalaryCycle;
using Application.Helpers;

namespace Application.QueryParams.ProjectSalaryCycle
{
    public class ProjectSalaryCycleQueryParams : PaginationParams
    {
        public ProjectSalaryCycleOrderBy OrderBy { get; set; } = ProjectSalaryCycleOrderBy.DateDesc;
        public List<ProjectSalaryCycleStatus> Status { get; set; } = new List<ProjectSalaryCycleStatus>();
    }
}