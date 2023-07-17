using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Application.Domain.Models
{
    public class ProjectReportMemberTask
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ProjectReportMemberTaskId { get; set; }

        public string TaskName { get; set; } = null!;
        public string TaskCode { get; set; } = null!;
        public string TaskDescription { get; set; } = null!;
        public double TaskHour { get; set; }
        public double TaskRealHour { get; set; }

        public double TaskPoint { get; set; }
        public double TaskEffort { get; set; }

        public double TaskBonus { get; set; }
        public String? BonusReason { get; set; }

        public Guid ProjectReportMemberId { get; set; }
        public virtual ProjectReportMember ProjectReportMember { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTimeHelper.Now();
    }
}