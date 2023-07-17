using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Project
{
  public class ProjectPMUpdateDTO
  {
    [Required]
    public Guid? ProjectId { get; set; }

    [StringLength(7)]
    public string? ProjectShortName { get; set; } = null!;
    public string? ProjectDescription { get; set; } = null!;
  }
}