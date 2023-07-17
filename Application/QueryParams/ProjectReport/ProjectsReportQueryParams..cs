using Application.Domain.Enums.ProjectReport;
using Application.Helpers;

namespace Application.QueryParams.ProjectReport
{
  public class ProjectsReportQueryParams : PaginationParams
  {
    public Guid? ProjectId { get; set; }
    public Guid? SalaryCycleId { get; set; }
    public ProjectReportOrderBy OrderBy { get; set; } = ProjectReportOrderBy.CreatedAtDesc;
    public List<ProjectReportStatus> Status { get; set; } = new List<ProjectReportStatus>();
  }
}