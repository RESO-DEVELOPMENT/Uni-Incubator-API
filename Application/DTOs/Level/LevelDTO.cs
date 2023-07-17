namespace Application.DTOs.Level
{
  public class LevelDTO
  {
    public int LevelId { get; set; }
    public string LevelName { get; set; } = null!;

    public double BasePoint { get; set; }
    public double BasePointPerHour { get; set; }
    //public double Loyal { get; set; }

    public double MinWorkHour { get; set; }
    public double MaxWorkHour { get; set; }
    
    public string LevelColor { get; set; } = null!;

    public double XPNeeded { get; set; }
  }
}