using Application.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Application.Persistence.Repositories
{
    public class SalaryCycleRepository : BaseRepository<SalaryCycle, Guid>
    {
        public SalaryCycleRepository(DataContext context) : base(context)
        {
        }

        public async Task<SalaryCycle?> GetLatestSalaryCycle()
        {
            return await table.OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync();
        }
    }
}