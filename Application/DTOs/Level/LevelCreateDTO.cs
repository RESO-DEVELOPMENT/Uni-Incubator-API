using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Level
{
    public class LevelCreateDTO
    {
        [Required]
        public string LevelName { get; set; } = null!;

        [Required]
        [Range(1, 100000)]
        public double BasePoint { get; set; }
        [Required]
        [Range(1, 100000)]
        public double BasePointPerHour { get; set; }
        //[Required]
        //[Range(1, 100000)]
        //public double Loyal { get; set; }
        [Required]
        [Range(0, 100000)]
        public double XPNeeded { get; set; }

        [Required]
        [Range(1, 100000)]
        public double MinWorkHour { get; set; }
        [Required]
        [Range(1, 100000)]
        public double MaxWorkHour { get; set; }

        [Required]
        public string LevelColor { get; set; } = null!;
    }
}