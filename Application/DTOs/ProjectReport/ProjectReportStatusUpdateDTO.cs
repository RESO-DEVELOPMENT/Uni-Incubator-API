using System.ComponentModel.DataAnnotations;
using Application.Domain.Enums.ProjectReport;

namespace Application.DTOs.ProjectReport
{
    public class ProjectReportStatusUpdateDTO
    {
        [Required]
        public Guid ReportId { get; set; }
        [Required]
        public ProjectReportStatus Status { get; set; }
        public string? Note { get; set; }
    }
}