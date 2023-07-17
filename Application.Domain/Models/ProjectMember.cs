using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Application.Domain.Enums.ProjectMember;

namespace Application.Domain.Models
{
  // [PrimaryKey(nameof(ProjectId), nameof(MemberId))]
  public class ProjectMember
  {
    public ProjectMember()
    {
      ProjectMemberAttributes = new List<ProjectMemberAttribute>();
      ProjectMemberReports = new List<ProjectReportMember>();
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid ProjectMemberId { get; set; }

    public Guid ProjectId { get; set; }
    public virtual Project Project { get; set; } = null!;

    public Guid MemberId { get; set; }
    public virtual Member Member { get; set; } = null!;

    public string Major { get; set; } = null!;

    public ProjectMemberRole Role { get; set; } = ProjectMemberRole.Member;
    public ProjectMemberStatus Status { get; set; } = ProjectMemberStatus.Active;

    public virtual List<ProjectMemberAttribute> ProjectMemberAttributes { get; set; }
    public virtual List<ProjectReportMember> ProjectMemberReports { get; set; }

    public DateTime CreatedAt { get; set; } = DateTimeHelper.Now();
    public DateTime? UpdatedAt { get; set; }
  }
}