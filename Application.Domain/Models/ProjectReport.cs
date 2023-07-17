using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Application.Domain.Enums.ProjectReport;

namespace Application.Domain.Models
{
    public class ProjectReport
    {
        public ProjectReport()
        {
            ProjectReportMembers = new List<ProjectReportMember>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ReportId { get; set; }

        public Guid ProjectId { get; set; }
        public virtual Project Project { get; set; } = null!;

        public string? Note { get; set; }
        public ProjectReportStatus Status { get; set; } = ProjectReportStatus.Created;

        //public double? ProjectPoint { get; set; } = 0;
        //public double? BonusPoint { get; set; } = 0;

        public Guid SalaryCycleId { get; set; }
        public virtual SalaryCycle SalaryCycle { get; set; } = null!;

        public virtual List<ProjectReportMember> ProjectReportMembers { get; set; }

        public DateTime CreatedAt { get; set; } = DateTimeHelper.Now();
        public DateTime? ReviewedAt { get; set; }

    }
}