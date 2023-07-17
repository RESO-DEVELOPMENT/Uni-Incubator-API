using Application.Domain.Models;

namespace Application.Persistence.Repositories
{
    public class ProjectSponsorTransactionRepository : BaseRepository<ProjectSponsorTransaction, Guid>
    {
        public ProjectSponsorTransactionRepository(DataContext context) : base(context)
        {
        }
    }
}