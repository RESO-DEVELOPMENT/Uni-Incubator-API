using Application.Domain.Enums.Project;
using Application.Helpers;

namespace Application.QueryParams.Member
{
    public class MemberProjectsQueryParams : PaginationParams
    {
        public string? ProjectName { get; set; }
        public List<ProjectStatus> Status { get; set; } = new List<ProjectStatus>();

        public DateTime? StartAfter { get; set; }
        public DateTime? EndBefore { get; set; }

        public MemberProjectsOrderBy OrderBy { get; set; } = MemberProjectsOrderBy.DateDesc;
    }
}