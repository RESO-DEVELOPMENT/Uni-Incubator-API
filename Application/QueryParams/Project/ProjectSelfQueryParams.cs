using Application.Domain.Enums.Project;
using Application.Helpers;

namespace Application.QueryParams.Project
{
    public class ProjectSelfQueryParams : PaginationParams
    {
        public string? ProjectName { get; set; }
        public List<ProjectStatus> Status { get; set; } = new List<ProjectStatus>();

        public ProjectOrderBy OrderBy { get; set; } = ProjectOrderBy.DateDesc;
    }
}