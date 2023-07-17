using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.ProjetMilestone
{
  public class ProjectMilestoneCreateDTO
  {
    [Required]
    public string Title { get; set; } = null!;
    [Required]
    public string Content { get; set; } = null!;
  }
}