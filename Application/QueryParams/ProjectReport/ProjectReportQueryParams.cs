using Application.Domain.Enums.ProjectReport;
using Application.Helpers;

namespace Application.QueryParams.ProjectReport
{
  public class ProjectReportQueryParams : PaginationParams
  {
    public Guid? SalaryCycleId { get; set; }
    public ProjectReportOrderBy OrderBy { get; set; } = ProjectReportOrderBy.CreatedAtDesc;
    public List<ProjectReportStatus> Status { get; set; } = new List<ProjectReportStatus>();
  }
}