using Application.Domain.Enums.Project;
using Application.Helpers;

namespace Application.QueryParams.Project
{
    public class ProjectForSponsorQueryParams : PaginationParams
    {
        public string? ProjectName { get; set; }
        public string? ManagerEmail { get; set; }
        public List<ProjectStatus> Status { get; set; } = new List<ProjectStatus>();

        public ProjectOrderBy OrderBy { get; set; } = ProjectOrderBy.DateDesc;

        public DateTime? StartAfter { get; set; }
        public DateTime? EndBefore { get; set; }

        public int? BudgetMin { get; set; }
        public int? BudgetMax { get; set; }
    }
}