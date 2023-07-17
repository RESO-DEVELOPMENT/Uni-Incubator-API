using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.ProjectReport
{
    public class ProjectReportDTO_Task
    {
        [EmailAddress]
        [Required]
        public string MemberEmail { get; set; } = null!;
        [Required]
        public string TaskName { get; set; } = null!; 
        [Required]
        public string TaskCode { get; set; } = null!;
        [Required]
        public string TaskDescription { get; set; } = null!;
        [Required]
        [Range(1, 100)]
        public double TaskHour { get; set; }
        [Range(1, 100)]
        [Required]
        public double TaskRealHour { get; set; }
        [Range(1, 10000)]
        [Required]
        public double TaskPoint { get; set; }
        [Range(1, 100)]
        [Required]
        public double TaskEffort { get; set; }

        [Required]
        [Range(0, 10000)]
        public double TaskBonus { get; set; }
        public string? BonusReason { get; set; }
    }
}
