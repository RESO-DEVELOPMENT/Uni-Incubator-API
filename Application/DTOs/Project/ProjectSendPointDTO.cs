using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Project
{
  public class ProjectSendPointDTO
  {
    [Required]
    [Range(1,100000)]
    public double Amount { get; set; }
  }
}