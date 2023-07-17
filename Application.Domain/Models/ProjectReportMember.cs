using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Application.Domain.Models
{
    public class ProjectReportMember
    {
        public ProjectReportMember()
        {
            ProjectReportMemberTasks = new List<ProjectReportMemberTask>();
            ProjectReportMemberAttributes = new List<ProjectReportMemberAttribute>();
        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ProjectReportMemberId { get; set; }

        public Guid ProjectReportId { get; set; }
        public virtual ProjectReport ProjectReport { get; set; } = null!;

        public Guid ProjectMemberId { get; set; }
        public virtual ProjectMember ProjectMember { get; set; } = null!;

        public virtual List<ProjectReportMemberAttribute> ProjectReportMemberAttributes { get; set; }
        public virtual List<ProjectReportMemberTask> ProjectReportMemberTasks { get; set; }
    }
}