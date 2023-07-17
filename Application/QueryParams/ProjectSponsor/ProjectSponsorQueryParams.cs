using Application.Domain.Enums.ProjectSponsor;
using Application.Helpers;

namespace Application.QueryParams.ProjectSponsor
{
    public class ProjectSponsorQueryParams : PaginationParams
    {
        public string? SponsorName { get; set; }
        public List<ProjectSponsorStatus> Status { get; set; } = new List<ProjectSponsorStatus>();
        public ProjectSponsorOrderBy OrderBy { get; set; } = ProjectSponsorOrderBy.DateDesc;
    }
}