using Application.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Application.Persistence.Repositories
{
    public class VoucherRepository : BaseRepository<Voucher, Guid>
    {
        public VoucherRepository(DataContext context) : base(context)
        {
        }

        public async Task<int> CountBySupplier(Guid supplierId)
        {
            return await table.Where(x => x.SupplierId == supplierId).CountAsync();
        }
    }
}