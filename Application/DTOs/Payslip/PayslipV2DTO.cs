using Application.Domain;
using Application.Domain.Enums.Payslip;
using Application.DTOs.Member;
using Application.DTOs.PayslipItem;
using Application.Helpers;

namespace Application.DTOs.Payslip
{
  public class PayslipV2DTO
  {

    public PayslipV2DTO()
    {
      Attributes = new Dictionary<string, string>();
    }
    
    public Guid PayslipId { get; set; }
    public string? Note { get; set; }

    public MemberDTO Member { get; set; } = null!;

    public double TotalP1 { get; set; }
    public double TotalP2 { get; set; }
    public double TotalP3 { get; set; }
    public double TotalXP { get; set; }
    public double TotalBonus { get; set; }

    public List<PayslipItemV2DTO> Items { get; set; } = new List<PayslipItemV2DTO>();
    public PayslipStatus Status { get; set; }

    public Dictionary<string, string> Attributes { get; set; }


    public DateTime CreatedAt { get; set; } = DateTimeHelper.Now();
  }
}