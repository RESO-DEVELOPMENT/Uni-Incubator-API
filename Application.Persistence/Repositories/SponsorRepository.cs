using Application.Domain.Models;

namespace Application.Persistence.Repositories
{
    public class SponsorRepository : BaseRepository<Sponsor, Guid>
    {
        public SponsorRepository(DataContext context) : base(context)
        {
        }
    }
}