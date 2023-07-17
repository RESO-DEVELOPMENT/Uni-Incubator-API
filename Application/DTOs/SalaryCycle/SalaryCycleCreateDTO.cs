using System.ComponentModel.DataAnnotations;
using static Application.Helpers.AttributeHelpers;

namespace Application.DTOs.SalaryCycle
{
    public class SalaryCycleCreateDTO
    {
        [ValidDate]
        [Required]
        public DateTime StartedAt { get; set; }

        [Required] public String Name { get; set; } = null!;
        public bool SendNotification { get; set; } = true;
    }
}