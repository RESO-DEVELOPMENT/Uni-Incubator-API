using Application.Domain.Enums.Payslip;
using Application.Helpers;

namespace Application.QueryParams.Payslip
{
  public class PayslipTotalQueryParams
  {
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public Guid? SalaryCycleId { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? MemberId { get; set; }
    public List<PayslipStatus> Status { get; set; } = new List<PayslipStatus>();
  }
}