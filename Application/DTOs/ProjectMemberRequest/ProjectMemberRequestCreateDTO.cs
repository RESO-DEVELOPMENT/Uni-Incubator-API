using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.ProjectMemberRequest
{
    public class ProjectMemberRequestCreateDTO
    {
        [Required]
        public string Major { get; set; } = null!;
        [Required]
        public string Note { get; set; } = null!;
    }
}