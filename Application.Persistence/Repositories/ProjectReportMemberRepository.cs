using Application.Domain.Models;

namespace Application.Persistence.Repositories
{
    public class ProjectReportMemberRepository : BaseRepository<ProjectReportMember, Guid>
    {
        public ProjectReportMemberRepository(DataContext context) : base(context)
        {
        }
    }
}