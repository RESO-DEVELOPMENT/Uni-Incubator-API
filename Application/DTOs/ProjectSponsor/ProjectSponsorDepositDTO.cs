using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.ProjectSponsor
{
    public class ProjectSponsorDepositDTO
    {
        [Required]
        public Guid ProjectSponsorId { get; set; }
        // [Required]
        // public Guid SalaryCycleId { get; set; }
        [Required]
        [Range(1,100000)]
        public double Amount { get; set; }
    }
}