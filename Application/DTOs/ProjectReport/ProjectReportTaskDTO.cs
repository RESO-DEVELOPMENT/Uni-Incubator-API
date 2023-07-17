using System.ComponentModel.DataAnnotations;
using Application.DTOs.Member;
using Application.DTOs.ProjectMember;
using Application.DTOs.ProjectReportMemberTask;

namespace Application.DTOs.ProjectReport
{
    public class ProjectReportTaskDTO
    {
        public Guid ProjectReportMemberTaskId { get; set; }
        //public MemberDTO Member { get; set; } = null!;
        public String MemberEmail { get; set; } = null!;

        public string TaskName { get; set; } = null!;
        public string TaskCode { get; set; } = null!;
        public string TaskDescription { get; set; } = null!;
        public double TaskHour { get; set; }
        public double TaskRealHour { get; set; }
        public double TaskEffort { get; set; }

        public double TaskPoint { get; set; }
        public double TaskBonus { get; set; }
        public string? BonusReason { get; set; }
    }
}