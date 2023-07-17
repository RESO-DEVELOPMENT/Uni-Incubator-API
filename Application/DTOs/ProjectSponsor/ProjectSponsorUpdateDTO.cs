using System.ComponentModel.DataAnnotations;
using Application.Domain.Enums.ProjectSponsor;

namespace Application.DTOs.ProjectSponsor
{
    public class ProjectSponsorUpdateDTO
    {
        [Required]
        public Guid ProjectSponsorId { get; set; }
        [Required]
        public ProjectSponsorStatus? Status { get; set; }
    }
}