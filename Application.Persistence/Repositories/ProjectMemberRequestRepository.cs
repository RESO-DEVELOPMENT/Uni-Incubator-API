using Application.Domain.Models;

namespace Application.Persistence.Repositories
{
    public class ProjectMemberRequestRepository : BaseRepository<ProjectMemberRequest, Guid>
    {
        public ProjectMemberRequestRepository(DataContext context) : base(context)
        {
        }
    }
}