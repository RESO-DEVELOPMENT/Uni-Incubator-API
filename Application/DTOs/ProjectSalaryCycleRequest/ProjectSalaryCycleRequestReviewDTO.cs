using System.ComponentModel.DataAnnotations;
using Application.Domain.Enums.ProjectSalaryCycleRequest;

namespace Application.DTOs.ProjectSalaryCycleRequest
{
    public class ProjectSalaryCycleRequestReviewDTO
    {
        [Required]
        public Guid RequestId { get; set; }
        [Required]
        public ProjectSalaryCycleRequestStatus Status { get; set; }
    }
}