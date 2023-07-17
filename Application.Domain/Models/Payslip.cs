using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Application.Domain.Enums.Payslip;

namespace Application.Domain.Models
{
  public class Payslip
  {
    public Payslip()
    {
      PayslipItems = new List<PayslipItem>();
      PayslipAttributes = new List<PayslipAttribute>();
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid PayslipId { get; set; }

    public Guid MemberId { get; set; }
    public virtual Member Member { get; set; } = null!;

    public Guid SalaryCycleId { get; set; }
    public SalaryCycle SalaryCycle { get; set; } = null!;
    public string? Note { get; set; }

    public PayslipStatus Status { get; set; }

    public virtual List<PayslipItem> PayslipItems { get; set; }
    public virtual List<PayslipAttribute> PayslipAttributes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTimeHelper.Now();
  }
}