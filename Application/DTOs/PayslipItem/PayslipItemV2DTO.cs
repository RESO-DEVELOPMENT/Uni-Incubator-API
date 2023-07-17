using Application.Domain;
using Application.Domain.Enums.PayslipItem;
using Application.Domain.Enums.Wallet;
using Application.DTOs.Project;
using Application.Helpers;

namespace Application.DTOs.PayslipItem
{
  public class PayslipItemV2DTO
  {
    public Guid PayslipItemId { get; set; }

    public WalletToken Token { get; set; }
    public double Amount { get; set; }
    public string? Note { get; set; }
    // public Guid? ProjectId { get; set; }
    public ProjectDTO Project { get; set; } = null;

    public PayslipItemType Type { get; set; }
    public Dictionary<string, string> Attributes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTimeHelper.Now();
  }
}