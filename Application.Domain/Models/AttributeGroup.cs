using System.ComponentModel.DataAnnotations;

namespace Application.Domain.Models
{
    public class AttributeGroup
    {
        [Key]
        public string AttributeGroupId { get; set; } = null!;
        public string AttributeGroupName { get; set; } = null!;
        public string AttributeGroupDescription { get; set; } = null!;

        public virtual List<Attribute> Attributes { get; set; } = null!;
    }
}