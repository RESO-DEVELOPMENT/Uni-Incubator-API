using Application.Domain.Models;

namespace Application.Persistence.Repositories
{
    public class ProjectRepository : BaseRepository<Project, Guid>
    {
        public ProjectRepository(DataContext context) : base(context)
        {
        }
    }
}