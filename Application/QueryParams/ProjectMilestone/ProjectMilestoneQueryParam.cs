using Application.Helpers;

namespace Application.QueryParams.ProjectMilestone
{
    public class ProjectUpdateQueryParams : PaginationParams
    {
        public ProjectMilestoneOrderBy OrderBy { get; set; } = ProjectMilestoneOrderBy.DateDesc;
    }
}