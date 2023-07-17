using System.ComponentModel.DataAnnotations;
using Application.DTOs.Project;

namespace Application.DTOs.ProjectReportMemberTask
{
    public class ProjectReportMemberTaskForPayslipDTO
    {
        public ProjectDTO Project { get; set; } = null!;
        public string ProjectReportMemberTaskId { get; set; } = null!;
        public string TaskName { get; set; } = null!;
        public string TaskCode { get; set; } = null!;
        public string TaskDescription { get; set; } = null!;
        public double TaskHour { get; set; }
        public double TaskRealHour { get; set; }
        public double TaskEffort { get; set; }

        public double TaskPoint { get; set; }
        public string? BonusReason { get; set; }
        public double TaskBonus { get; set; }
    }
}