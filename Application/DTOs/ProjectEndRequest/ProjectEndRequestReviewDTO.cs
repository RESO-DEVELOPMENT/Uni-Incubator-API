using Application.Domain;
using Application.Domain.Enums.ProjectEndRequest;
using Application.DTOs.Project;
using Microsoft.Build.Framework;

namespace Application.DTOs.ProjectEndRequest
{
    public class ProjectEndRequestReviewDTO
    {
        [Required]
        public Guid RequestId { get; set; }
        [Required]
        public ProjectEndRequestStatus Status { get; set; }
    }
}
