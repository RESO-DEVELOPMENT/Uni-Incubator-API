using Microsoft.EntityFrameworkCore;

namespace Application.Domain.Models
{
    [PrimaryKey(nameof(PayslipItemId), nameof(AttributeId))]
    public class PayslipItemAttribute
    {
        public Guid PayslipItemId { get; set; }
        public virtual PayslipItem PayslipItem { get; set; } = null!;

        public Guid AttributeId { get; set; }
        public virtual Attribute Attribute { get; set; } = null!;
    }
}