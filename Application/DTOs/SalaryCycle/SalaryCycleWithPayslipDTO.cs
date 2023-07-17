using Application.Domain.Enums.SalaryCycle;
using Application.DTOs.Payslip;

namespace Application.DTOs.SalaryCycle
{
  public class SalaryCycleWithPayslipDTO
  {
    public Guid SalaryCycleId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }

    public String Name { get; set; }

    public double TotalPoint { get; set; }
    public List<PayslipDTO> Payslips { get; set; } = new List<PayslipDTO>();
    public SalaryCycleStatus Status { get; set; }
  }
}