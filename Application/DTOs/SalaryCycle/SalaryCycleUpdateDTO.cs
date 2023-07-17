using System.ComponentModel.DataAnnotations;
using Application.Domain.Enums.SalaryCycle;

namespace Application.DTOs.SalaryCycle
{
    public class SalaryCycleUpdateDTO
    {
        [Required]
        public Guid SalaryCycleId { get; set; }
        [Required]
        public SalaryCycleStatus Status { get; set; }
    }
}