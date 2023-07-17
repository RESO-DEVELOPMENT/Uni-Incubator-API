using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.ProjectReportMemberTask
{
    public class ProjectReportMemberTaskDTO
    {
        public string ProjectReportMemberTaskId { get; set; } = null!;
        public string TaskName { get; set; } = null!;
        public string TaskCode { get; set; } = null!;
        public string TaskDescription { get; set; } = null!;
        public double TaskHour { get; set; }
        public double TaskRealHour { get; set; }
        public double TaskEffort { get; set; }
    }
}