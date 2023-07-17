using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Application.Domain.Enums.Wallet;

namespace Application.Domain.Models
{
    public class Transaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid TransactionId { get; set; }

        public Guid? PayslipItemId { get; set; }
        public PayslipItem? PayslipItem { get; set; } = null!;

        public Guid FromWalletId { get; set; }
        public Wallet FromWallet { get; set; } = null!;

        public Guid ToWalletId { get; set; }
        public Wallet ToWallet { get; set; } = null!;

        public TransactionType TransactionType { get; set; }
        public string? Note { get; set; } = null!;

        public WalletToken Token { get; set; }
        public double Amount { get; set; }

        public double FromAmountAfterTransaction { get; set; }
        public double ToAmountAfterTransaction { get; set; }

        public DateTime CreatedAt { get; set; } = DateTimeHelper.Now();
    }
}