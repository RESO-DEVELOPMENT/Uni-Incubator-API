using Application.Domain.Enums.Wallet;

namespace Application.DTOs.Wallet
{
  public class WalletDTO
  {
    public Guid WalletId { get; set; }
    // public Guid UserId { get; set; }

    public double Amount { get; set; }

    public WalletToken WalletToken { get; set; }
    public WalletType WalletType { get; set; }
    // public WalletStatus WalletStatus { get; set; }

    public String? WalletTag { get; set; }

    public DateTime ExpiredDate { get; set; }
  }
}