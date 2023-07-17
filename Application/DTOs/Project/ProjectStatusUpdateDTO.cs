using System.ComponentModel.DataAnnotations;
using Application.Domain.Enums.Project;

namespace Application.DTOs.Project
{
    public class ProjectStatusUpdateDTO
    {
        [Required]
        public Guid ProjectId { get; set; }
        [Required]
        public ProjectStatus ProjectStatus { get; set; }
    }
}