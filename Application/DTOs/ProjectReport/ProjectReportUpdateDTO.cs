using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.ProjectReport
{
    public class ProjectReportUpdateDTO
    {
        [Required]
        public Guid ProjectReportId { get; set; }
        [Required]
        public List<ProjectReportDTO_Task> MemberTasks { get; set; } = new List<ProjectReportDTO_Task>();
    }
}