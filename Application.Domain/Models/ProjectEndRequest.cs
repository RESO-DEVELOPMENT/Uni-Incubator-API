using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Application.Domain.Enums.ProjectEndRequest;
using Application.Domain.Enums.ProjectMemberRequest;

namespace Application.Domain.Models
{
    public class ProjectEndRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid RequestId { get; set; }

        public Guid ProjectId { get; set; }
        public virtual Project Project { get; set; } = null!;

        public ProjectEndRequestStatus Status { get; set; } = ProjectEndRequestStatus.Created;

        public ProjectEndRequestPointAction PointAction { get; set; } =
            ProjectEndRequestPointAction.PointReturnedToSystem;

        public DateTime CreatedAt { get; set; } = DateTimeHelper.Now();
        public DateTime? ReviewedAt { get; set; }
        public string? Note { get; set; } = null!;
    }
}