using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Application.Domain.Enums.ProjectMemberRequest;

namespace Application.Domain.Models
{
    public class ProjectMemberRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid RequestId { get; set; }

        public Guid ProjectId { get; set; }
        public virtual Project Project { get; set; } = null!;

        public Guid MemberId { get; set; }
        public virtual Member Member { get; set; } = null!;

        public string Major { get; set; } = null!;
        public string Note { get; set; } = null!;

        public ProjectMemberRequestStatus Status { get; set; } = ProjectMemberRequestStatus.Created;

        public DateTime CreatedAt { get; set; } = DateTimeHelper.Now();
        public DateTime? ReviewedAt { get; set; }
    }
}