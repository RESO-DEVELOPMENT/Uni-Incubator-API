using Application.Domain.Models;

namespace Application.Persistence.Repositories
{
    public class ProjectMilestoneRepository : BaseRepository<ProjectMilestone, Guid>
    {
        public ProjectMilestoneRepository(DataContext context) : base(context)
        {
        }
    }
}