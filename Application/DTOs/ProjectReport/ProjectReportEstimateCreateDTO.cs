using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.ProjectReport
{
  public class ProjectReportEstimateCreateDTO
  {
    [Required]
    public Guid ProjectId { get; set; }
    [Required]
    public Guid SalaryCycleId { get; set; }

    public Guid? ProjectReportId { get; set; }

    public List<ProjectReportDTO_Task> MemberTasks { get; set; } = new List<ProjectReportDTO_Task>();
  }
}