using Application.Domain.Enums.ProjectReport;

namespace Application.DTOs.ProjectReport
{
  public class ProjectReportDTO
  {
    public Guid ReportId { get; set; }
    public Guid ProjectId { get; set; }
    public Guid SalaryCycleId { get; set; }

    public ProjectReportStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }

    public string? Note { get; set; }

    public double ProjectPoint { get; set; }
    public double BonusPoint { get; set; }
  }
}