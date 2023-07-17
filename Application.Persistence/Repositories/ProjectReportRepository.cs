using Application.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Application.Persistence.Repositories
{
    public class ProjectReportRepository : BaseRepository<ProjectReport, Guid>
    {
        public ProjectReportRepository(DataContext context) : base(context)
        {
        }

        public IQueryable<ProjectReport> GetReportFullQuery(Guid reportId)
        {
            var query = this.GetQuery();

            return query
                .Include(pr => pr.SalaryCycle)
                  .ThenInclude(sc => sc.Payslips)
                    .ThenInclude(ps => ps.PayslipAttributes)
                      .ThenInclude(ps => ps.Attribute)

                .Include(pr => pr.ProjectReportMembers)
                  .ThenInclude(prm => prm.ProjectReportMemberTasks)

                .Include(pr => pr.Project)
                  .ThenInclude(p => p.ProjectMember)
                    .ThenInclude(pm => pm.ProjectMemberAttributes)
                      .ThenInclude(pma => pma.Attribute)

                .Include(pr => pr.ProjectReportMembers)
                    .ThenInclude(x => x.ProjectReportMemberAttributes)
                        .ThenInclude(x => x.Attribute)

                .Include(pr => pr.ProjectReportMembers)
                    .ThenInclude(prm => prm.ProjectMember.Member.MemberLevels.Where(ml => ml.IsActive))
                      .ThenInclude(ml => ml.Level)

                  .Where(pr => pr.ReportId == reportId);
        }

        public IQueryable<ProjectReport> GetReportsFullQuery()
        {
            var query = this.GetQuery();

            return query
                .Include(pr => pr.SalaryCycle)
                  .ThenInclude(sc => sc.Payslips)
                    .ThenInclude(ps => ps.PayslipAttributes)
                      .ThenInclude(ps => ps.Attribute)

                .Include(pr => pr.ProjectReportMembers)
                  .ThenInclude(prm => prm.ProjectReportMemberTasks)

                .Include(pr => pr.Project)
                  .ThenInclude(p => p.ProjectMember)
                    .ThenInclude(pm => pm.ProjectMemberAttributes)
                      .ThenInclude(pma => pma.Attribute)

                .Include(pr => pr.ProjectReportMembers)
                    .ThenInclude(x => x.ProjectReportMemberAttributes)
                        .ThenInclude(x => x.Attribute)

                  .Include(pr => pr.ProjectReportMembers)
                    .ThenInclude(prm => prm.ProjectMember.Member.MemberLevels.Where(ml => ml.IsActive))
                      .ThenInclude(ml => ml.Level);
        }
    }
}