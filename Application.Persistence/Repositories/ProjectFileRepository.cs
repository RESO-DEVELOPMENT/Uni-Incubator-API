using Application.Domain.Models;

namespace Application.Persistence.Repositories
{
    public class ProjectFileRepository : BaseRepository<ProjectFile, Guid>
    {
        public ProjectFileRepository(DataContext context) : base(context)
        {
        }
    }
}