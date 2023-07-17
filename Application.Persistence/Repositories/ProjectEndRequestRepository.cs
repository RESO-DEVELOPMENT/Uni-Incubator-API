using Application.Domain.Models;

namespace Application.Persistence.Repositories
{
    public class ProjectEndRequestRepository : BaseRepository<ProjectEndRequest, Guid>
    {
        public ProjectEndRequestRepository(DataContext context) : base(context)
        {
        }
    }
}