using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Level
{
  public class LevelCreateBulkDTO
  {
    [Required]
    public List<LevelCreateDTO> Levels { get; set; } = new List<LevelCreateDTO>();
  }
}