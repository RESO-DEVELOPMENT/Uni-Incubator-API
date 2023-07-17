using Application.Domain.Models;

namespace Application.Persistence.Repositories
{
    public class ProjectSponsorRepository : BaseRepository<ProjectSponsor, Guid>
    {
        public ProjectSponsorRepository(DataContext context) : base(context)
        {
        }
    }
}