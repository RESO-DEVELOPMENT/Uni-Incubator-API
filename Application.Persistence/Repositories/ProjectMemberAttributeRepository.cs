using Application.Domain.Models;

namespace Application.Persistence.Repositories
{
    public class ProjectMemberAttributeRepository : BaseRepository<ProjectMemberAttribute, Guid>
    {
        public ProjectMemberAttributeRepository(DataContext context) : base(context)
        {
        }
    }
}