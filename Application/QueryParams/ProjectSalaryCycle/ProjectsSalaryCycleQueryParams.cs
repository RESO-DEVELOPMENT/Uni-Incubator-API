using Application.Domain.Enums.ProjectSalaryCycle;
using Application.Helpers;

namespace Application.QueryParams.ProjectSalaryCycle
{
    public class ProjectsSalaryCycleQueryParams : PaginationParams
    {
        public Guid? ProjectId { get; set; }
        public ProjectSalaryCycleOrderBy OrderBy { get; set; } = ProjectSalaryCycleOrderBy.DateDesc;
        public List<ProjectSalaryCycleStatus> Status { get; set; } = new List<ProjectSalaryCycleStatus>();
    }
}