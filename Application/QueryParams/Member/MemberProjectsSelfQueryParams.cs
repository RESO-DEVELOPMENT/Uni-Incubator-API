using Application.Domain.Enums.Project;
using Application.Helpers;

namespace Application.QueryParams.Member
{
  public class MemberProjectsSelfQueryParams : PaginationParams
  {
    public string? ProjectName { get; set; }
    public List<ProjectStatus> Status { get; set; } = new List<ProjectStatus>();

    public DateTime? StartAfter { get; set; }
    public DateTime? EndBefore { get; set; }

    public int? BudgetMin { get; set; }
    public int? BudgetMax { get; set; }

    public bool? IsManager { get; set; }

    public MemberProjectsOrderBy OrderBy { get; set; } = MemberProjectsOrderBy.DateDesc;
  }
}