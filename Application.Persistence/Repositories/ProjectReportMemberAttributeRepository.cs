using Application.Domain.Models;

namespace Application.Persistence.Repositories
{
    public class ProjectReportMemberAttributeRepository : BaseRepository<ProjectReportMemberAttribute, Guid>
    {
        public ProjectReportMemberAttributeRepository(DataContext context) : base(context)
        {
        }
    }
}