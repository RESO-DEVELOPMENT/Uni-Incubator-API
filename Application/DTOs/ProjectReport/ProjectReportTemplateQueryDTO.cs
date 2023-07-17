using System.ComponentModel.DataAnnotations;
using Application.Domain.Enums.ProjectReport;

namespace Application.DTOs.ProjectReport
{
    public class ProjectReportTemplateQueryDTO
    {
        [Required]
        public Guid ProjectId { get; set; }
        [Required]
        public Guid SalaryCycleId { get; set; }
    }
}