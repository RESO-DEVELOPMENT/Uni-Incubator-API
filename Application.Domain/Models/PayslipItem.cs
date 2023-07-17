using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Application.Domain.Enums.PayslipItem;
using Application.Domain.Enums.Wallet;

namespace Application.Domain.Models
{
    public class PayslipItem
    {
        public PayslipItem()
        {
            Transactions = new List<Transaction>();
            PayslipItemAttributes = new List<PayslipItemAttribute>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid PayslipItemId { get; set; }

        public Guid PayslipId { get; set; }
        public virtual Payslip Payslip { get; set; } = null!;

        public WalletToken Token { get; set; }
        public double Amount { get; set; }
        public string? Note { get; set; }

        public Guid? ProjectId { get; set; }
        public virtual Project? Project { get; set; }

        public PayslipItemType Type { get; set; }

        public virtual List<Transaction> Transactions { get; set; }
        public virtual List<PayslipItemAttribute> PayslipItemAttributes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTimeHelper.Now();
    }
}