using Microsoft.EntityFrameworkCore;

namespace Application.Domain.Models
{
    [PrimaryKey(nameof(ProjectId), nameof(WalletId))]
    public class ProjectWallet
    {
        public Guid ProjectId { get; set; }
        public virtual Project Project { get; set; } = null!;

        public Guid WalletId { get; set; }
        public virtual Wallet Wallet { get; set; } = null!;
    }
}