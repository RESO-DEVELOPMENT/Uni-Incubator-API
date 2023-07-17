using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.ProjectSponsor
{
    public class ProjectSponsorCreateDTO
    {
        [Required]
        public Guid SponsorId { get; set; }
    }
}