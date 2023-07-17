using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.ProjetMilestone
{
  public class ProjectMilestoneUpdateDTO
  {
    [Required]
    public Guid MilestoneId { get; set; }
    [Required]
    public string Title { get; set; } = null!;
    [Required]
    public string Content { get; set; } = null!;
  }
}