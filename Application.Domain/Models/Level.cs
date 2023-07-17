using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Application.Domain.Enums.Level;

namespace Application.Domain.Models
{
  public class Level
  {
    public Level()
    {
      MemberLevels = new List<MemberLevel>();
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int LevelId { get; set; }
    public string LevelName { get; set; } = null!;

    public double BasePoint { get; set; }
    public double BasePointPerHour { get; set; }
    //public double Loyal { get; set; }
    public LevelStatus Status { get; set; } = LevelStatus.Active;

    public double MinWorkHour { get; set; }
    public double MaxWorkHour { get; set; }

    public string LevelColor { get; set; } = null!;
    public double XPNeeded { get; set; }

    public DateTime CreatedAt { get; set; } = DateTimeHelper.Now();

    public virtual List<MemberLevel> MemberLevels { get; set; }
  }
}