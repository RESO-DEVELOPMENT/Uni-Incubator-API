using System.ComponentModel.DataAnnotations;
using Application.Domain.Enums.Project;

namespace Application.DTOs.Project
{
  public class ProjectAdminUpdateDTO
  {
    [Required]
    public Guid ProjectId { get; set; }
    public string? ProjectName { get; set; } = null!;
    [MaxLength(7)]
    public string? ProjectShortName { get; set; } = null!;
    public string? ProjectDescription { get; set; } = null!;

    public ProjectVisibility? ProjectVisibility { get; set; }

    [Range(0, 1000000)]
    public double? Budget { get; set; }
  }
}