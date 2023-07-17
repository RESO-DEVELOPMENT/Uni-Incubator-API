using Application.Domain.Models;

namespace Application.Persistence.Repositories
{
    public class ProjectReportMemberTaskRepository : BaseRepository<ProjectReportMemberTask, Guid>
    {
        public ProjectReportMemberTaskRepository(DataContext context) : base(context)
        {
        }
    }
}