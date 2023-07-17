using Application.Domain.Enums.Wallet;

namespace Application.DTOs.Transaction
{
  public class TransactionDTO
  {
    public Guid TransactionId { get; set; }

    public Guid FromWalletId { get; set; }
    public Guid ToWalletId { get; set; }

    public TransactionType TransactionType { get; set; }
    public string? Note { get; set; } = null!;

    public WalletToken Token { get; set; }
    public double Amount { get; set; }

    public bool IsReceived { get; set; } = true;
    public double AmountLeft { get; set; }

    public DateTime CreatedAt { get; set; }
  }
}