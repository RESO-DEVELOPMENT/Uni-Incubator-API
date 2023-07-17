using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Domain.Models
{
    [PrimaryKey(nameof(ProjectReportMemberId), nameof(AttributeId))]
    public class ProjectReportMemberAttribute
    {
        public Guid ProjectReportMemberId { get; set; }
        public virtual ProjectReportMember ProjectReportMember { get; set; } = null!;

        public Guid AttributeId { get; set; }
        public virtual Attribute Attribute { get; set; } = null!;
    }
}
