using Application.Domain.Enums.ProjectReport;
using Application.DTOs.Project;
using Application.DTOs.SalaryCycle;

namespace Application.DTOs.ProjectReport
{
    public class ProjectReportWithProjectAndSalaryCycleDTO
    {
        public Guid ReportId { get; set; }

        public SalaryCycleDTO SalaryCycle { get; set; } = null!;
        public ProjectWithFilesDTO Project { get; set; } = null!;

        public ProjectReportStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }

        public string? Note { get; set; }

        public double TaskPointNeeded { get; set; }
        public double BonusPointNeeded { get; set; }
    }
}