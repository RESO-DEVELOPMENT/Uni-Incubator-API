using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Application.Domain.Enums.ProjectMilestone;

namespace Application.Domain.Models
{
  // [PrimaryKey(nameof(ProjectId), nameof(MemberId))]
  public class ProjectMilestone
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid ProjectMilestoneId { get; set; }

    public Guid ProjectId { get; set; }
    public virtual Project Project { get; set; } = null!;

    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public ProjectMilestoneStatus Status { get; set; }

    public DateTime CreatedAt { get; set; } = DateTimeHelper.Now();
  }
}