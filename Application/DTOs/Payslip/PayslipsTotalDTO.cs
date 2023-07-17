using Application.DTOs.Member;
using Application.DTOs.PayslipItem;
using Application.Helpers;

namespace Application.DTOs.Payslip
{
  public class PayslipsTotalDTO
  {
    public int PayslipsCount { get; set; }

    public double TotalP1 { get; set; }
    public double TotalP2 { get; set; }
    public double TotalP3 { get; set; }
    public double TotalBonus { get; set; }
    public double TotalXP { get; set; }
  }
}