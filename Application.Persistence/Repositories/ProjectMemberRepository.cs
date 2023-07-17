using Application.Domain.Enums.ProjectMember;
using Application.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Application.Persistence.Repositories
{
    public class ProjectMemberRepository : BaseRepository<ProjectMember, Guid>
    {
        public ProjectMemberRepository(DataContext context) : base(context)
        {
        }

        public async Task<ProjectMember?> TryGetProjectMemberManagerActive(Guid projectId, String email)
        {
            return await table
              .Include(m => m.Member)
             .Where(m => m.Member.EmailAddress == email &&
             m.Status == ProjectMemberStatus.Active &&
             m.ProjectId == projectId &&
             m.Role == ProjectMemberRole.Manager)
             .FirstOrDefaultAsync();
        }

        public async Task<ProjectMember?> TryGetProjectMemberManagerActive(Guid projectId)
        {
            return await table
                .Include(m => m.Member)
                .Where(m =>
                            m.Status == ProjectMemberStatus.Active &&
                            m.ProjectId == projectId &&
                            m.Role == ProjectMemberRole.Manager)
                .FirstOrDefaultAsync();
        }

        public async Task<ProjectMember?> TryGetProjectMemberActive(Guid projectId, String email)
        {
            return await table
                .Include(m => m.Member)
                .Where(m => m.Member.EmailAddress == email &&
                            m.Status == ProjectMemberStatus.Active &&
                            m.ProjectId == projectId)
                .FirstOrDefaultAsync();
        }
    }
}