using System.ComponentModel.DataAnnotations;
using Application.Domain.Enums.ProjectSalaryCycleRequest;

namespace Application.DTOs.ProjectSalaryCycleRequest
{
    public class ProjectSalaryCycleRequestCreateDTO
    {
        [Required]
        public ProjectSalaryCycleRequestType RequestType { get; set; }
        [Required]
        public string Value { get; set; } = null!;
    }
}