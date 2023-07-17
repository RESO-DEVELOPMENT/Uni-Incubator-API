using Application.Domain.Enums.PayslipItem;
using Application.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Application.Persistence.Repositories
{
  public class PayslipRepository : BaseRepository<Payslip, Guid>
  {
    public PayslipRepository(DataContext context) : base(context)
    {
    }

    public IQueryable<Payslip> GetPaySlipQueryFull()
    {
      return table
        .Include(p => p.PayslipItems)
        .Include(p => p.Member)
        .Include(p => p.SalaryCycle)
        .Include(p => p.PayslipAttributes)
        .ThenInclude(pa => pa.Attribute)
        .Include(p => p.PayslipItems)
        .ThenInclude(x => x.Project)
        .Include(p => p.PayslipItems)
        .ThenInclude(pa => pa.PayslipItemAttributes)
        .ThenInclude(pa => pa.Attribute);
    }

    public IQueryable<Payslip> GetPaySlipQueryFullForProject(Guid projectId)
    {
      return table
          .Include(p => p.PayslipItems
              .Where(p => p.Type != PayslipItemType.P1 && p.ProjectId == projectId)
          )

          .Include(p => p.Member)
          .Include(p => p.SalaryCycle)
          .Include(p => p.PayslipAttributes)
          .ThenInclude(pa => pa.Attribute)

          .Include(p => p.PayslipItems)
          .ThenInclude(x => x.Project)

          .Include(p => p.PayslipItems
              .Where(p => p.Type != PayslipItemType.P1 && p.ProjectId == projectId))
          .ThenInclude(pa => pa.PayslipItemAttributes)
          .ThenInclude(pa => pa.Attribute)

          .Where(p =>
              p.PayslipItems.Any(psi => (psi.Type == PayslipItemType.P2 || psi.Type == PayslipItemType.P3) && psi.ProjectId == projectId)
          );
    }

    public IQueryable<Payslip> GetPaySlipQueryFullForProjectOrNull(Guid? projectId)
    {
      return projectId == null ? GetPaySlipQueryFull() : GetPaySlipQueryFullForProject(projectId.Value);
    }
  }
}