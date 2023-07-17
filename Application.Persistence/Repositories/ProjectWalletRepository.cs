using Application.Domain.Models;

namespace Application.Persistence.Repositories
{
    public class ProjectWalletRepository : BaseRepository<ProjectWallet, Guid>
    {
        public ProjectWalletRepository(DataContext context) : base(context)
        {
        }
    }
}