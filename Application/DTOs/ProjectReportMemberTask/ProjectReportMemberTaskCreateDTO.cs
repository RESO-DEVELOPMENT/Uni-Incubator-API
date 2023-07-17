using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.ProjectReportMemberTask
{
    public class ProjectReportMemberTaskCreateDTO
    {
        // [Required]
        // public String MemberEmail { get; set; } = null!;
        [Required]
        public string TaskName { get; set; } = null!;
        [Required]
        public string TaskCode { get; set; } = null!;
        [Required]
        public string TaskDescription { get; set; } = null!;
        [Required]
        public double TaskHour { get; set; }
        [Required]
        public double TaskRealHour { get; set; }
        [Required]
        public double TaskEffort { get; set; }
    }
}