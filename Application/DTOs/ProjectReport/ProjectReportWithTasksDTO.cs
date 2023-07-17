using Application.Domain.Enums.ProjectReport;
using Application.DTOs.ProjectReportMember;

namespace Application.DTOs.ProjectReport
{
    public class ProjectReportWithTasksDTO
    {
        public List<ProjectReportTaskDTO> Tasks { get; set; } = new List<ProjectReportTaskDTO>();

        public Guid ReportId { get; set; }
        public Guid ProjectId { get; set; }
        public Guid SalaryCycleId { get; set; }

        public ProjectReportStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }

        public string? Note { get; set; }

        public double TaskPointNeeded { get; set; }
        public double BonusPointNeeded { get; set; }
    }
}