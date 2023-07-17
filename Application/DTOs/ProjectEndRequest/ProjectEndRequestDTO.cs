using Application.Domain;
using Application.Domain.Enums.ProjectEndRequest;
using Application.DTOs.Project;

namespace Application.DTOs.ProjectEndRequest
{
    public class ProjectEndRequestDTO
    {
        public Guid RequestId { get; set; }
        public virtual ProjectDTO Project { get; set; } = null!;
        public string? Note { get; set; } = null!;

        public ProjectEndRequestStatus Status { get; set; }
        public ProjectEndRequestPointAction PointAction { get; set; }

        public DateTime CreatedAt { get; set; } 
        public DateTime? ReviewedAt { get; set; }
    }
}
