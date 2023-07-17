using Application.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Application.Persistence.Repositories
{
    public class MemberVoucherRepository : BaseRepository<MemberVoucher, Guid>
    {
        public MemberVoucherRepository(DataContext context) : base(context)
        {
        }

        public async Task<MemberVoucher?> GetVoucherFromCode(string code)
        {
           return await table.FirstOrDefaultAsync(x => x.Code == code);
        }
    }
}