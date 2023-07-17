using Microsoft.EntityFrameworkCore;

namespace Application.Domain.Models
{
    [PrimaryKey(nameof(ProjectMemberId), nameof(AttributeId))]
    public class ProjectMemberAttribute
    {
        public Guid ProjectMemberId { get; set; }
        public virtual ProjectMember ProjectMember { get; set; } = null!;

        public Guid AttributeId { get; set; }
        public virtual Attribute Attribute { get; set; } = null!;
    }
}