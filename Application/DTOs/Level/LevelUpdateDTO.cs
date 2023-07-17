using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Level
{
  public class LevelUpdateDTO
  {
    [Required]
    public int LevelId { get; set; }
    [Required]
    public string LevelName { get; set; } = null!;
    [Range(1, 100000)]
    [Required]
    public double BasePoint { get; set; }
    [Range(1, 100000)]
    [Required]
    public double BasePointPerHour { get; set; }
    //[Range(1, 100000)]
    //[Required]
    //public double Loyal { get; set; }
    [Range(0, 100000)]
    [Required]
    public double XPNeeded { get; set; }

    [Range(1, 100000)]
    [Required]
    public double MinWorkHour { get; set; }
    [Range(1, 100000)]
    [Required]
    public double MaxWorkHour { get; set; }
    [Required]
    public string LevelColor { get; set; } = null!;
  }
}