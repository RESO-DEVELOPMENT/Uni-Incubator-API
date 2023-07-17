using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Application.Domain.Models
{
    public class Attribute
    {
        public Attribute()
        {
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid AttributeId { get; set; }

        public string AttributeGroupId { get; set; } = null!;
        public AttributeGroup AttributeGroup { get; set; } = null!;

        public string Value { get; set; } = null!;

        public virtual List<ProjectMemberAttribute>? ProjectMemberAttributes { get; set; }
        public virtual List<PayslipAttribute>? PayslipAttributes { get; set; }
        public virtual List<PayslipItemAttribute>? PayslipItemAttributes { get; set; }
        public virtual List<ProjectReportMemberAttribute>? ProjectReportMemberAttributes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTimeHelper.Now();
        // [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        // public DateTime UpdatedAt { get; set; } = DateTimeHelper.Now();
    }
}
