using Application.Domain.Models;

namespace Application.Persistence.Repositories
{
    public class MemberWalletRepository : BaseRepository<MemberWallet, Guid>
    {
        public MemberWalletRepository(DataContext context) : base(context)
        {
        }
    }
}