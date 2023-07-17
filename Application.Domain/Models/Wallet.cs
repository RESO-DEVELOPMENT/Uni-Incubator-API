using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Application.Domain.Enums.Wallet;

namespace Application.Domain.Models
{
  public class Wallet
  {
    public Wallet()
    {
      TransactionsFrom = new List<Transaction>();
      TransactionsTo = new List<Transaction>();
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid WalletId { get; set; }

    public double Amount { get; set; }
    public bool IsSystem { get; set; } = false;
    public TargetType TargetType { get; set; }

    public WalletToken WalletToken { get; set; }
    public WalletType WalletType { get; set; }
    public WalletStatus WalletStatus { get; set; }

    public DateTime ExpiredDate { get; set; }

    public String? WalletTag { get; set; }

    public virtual MemberWallet? MemberWallet { get; set; } = null!;
    public virtual ProjectWallet? ProjectWallet { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTimeHelper.Now();
    // Send
    public virtual List<Transaction> TransactionsFrom { get; set; }
    // Recieved
    public virtual List<Transaction> TransactionsTo { get; set; }
  }
}