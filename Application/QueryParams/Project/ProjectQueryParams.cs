using Application.Domain.Enums.Project;
using Application.Helpers;

namespace Application.QueryParams.Project
{
    public class ProjectQueryParams : PaginationParams
    {
        public string? ProjectName { get; set; }
        public string? ManagerEmail { get; set; }
        public List<ProjectStatus> Status { get; set; } = new List<ProjectStatus>();

        public bool IncludePrivate { get; set; } = false;
        public ProjectOrderBy OrderBy { get; set; } = ProjectOrderBy.DateDesc;

        public DateTime? StartAfter { get; set; }
        public DateTime? EndBefore { get; set; }

        public int? BudgetMin { get; set; }
        public int? BudgetMax { get; set; }
    }
}