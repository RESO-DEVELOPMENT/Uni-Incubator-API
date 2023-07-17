using Application.Domain.Models;

namespace Application.Persistence.Repositories
{
    public class UserWalletRepository : BaseRepository<MemberWallet, Guid>
    {
        public UserWalletRepository(DataContext context) : base(context)
        {
        }
    }
}