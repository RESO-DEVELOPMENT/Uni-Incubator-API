using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.ProjectMemberRequest
{
    public class ProjectMemberChangePMDTO
    {
        [Required]
        public string Email { get; set; } = null!;
    }
}