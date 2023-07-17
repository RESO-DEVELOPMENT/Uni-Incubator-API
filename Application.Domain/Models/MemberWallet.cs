using Microsoft.EntityFrameworkCore;

namespace Application.Domain.Models
{
    [PrimaryKey(nameof(MemberId), nameof(WalletId))]
    public class MemberWallet
    {
        public Guid MemberId { get; set; }
        public virtual Member Member { get; set; } = null!;

        public Guid WalletId { get; set; }
        public virtual Wallet Wallet { get; set; } = null!;
    }
}