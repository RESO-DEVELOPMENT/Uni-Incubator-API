using Microsoft.EntityFrameworkCore;

namespace Application.Domain.Models
{
  [PrimaryKey(nameof(PayslipId), nameof(AttributeId))]
  public class PayslipAttribute
  {
    public Guid PayslipId { get; set; }
    public virtual Payslip Payslip { get; set; } = null!;

    public Guid AttributeId { get; set; }
    public virtual Attribute Attribute { get; set; } = null!;
  }
}