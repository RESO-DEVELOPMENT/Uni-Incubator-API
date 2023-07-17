using Application.Domain.Models;

namespace Application.Persistence.Repositories
{
    public class PayslipItemRepository : BaseRepository<PayslipItem, Guid>
    {
        public PayslipItemRepository(DataContext context) : base(context)
        {
        }
    }
}