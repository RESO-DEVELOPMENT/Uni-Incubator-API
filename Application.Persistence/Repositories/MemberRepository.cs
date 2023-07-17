using Application.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Application.Persistence.Repositories
{
    public class MemberRepository : BaseRepository<Member, Guid>
    {
        public MemberRepository(DataContext context) : base(context)
        {
        }

        public async Task<Member?> GetByEmail(String email)
        {
            return await table.FirstOrDefaultAsync(m => m.EmailAddress == email);
        }

        public async Task<Member?> GetByEmailWithUser(String email)
        {
            return await table.Include(m => m.User).FirstOrDefaultAsync(m => m.EmailAddress == email);
        }

        public async Task<Member?> GetByIDWithUser(Guid memberId)
        {
            return await table.Include(m => m.User).FirstOrDefaultAsync(m => m.MemberId == memberId);
        }

        public async Task<List<Member>> GetActiveAdmins()
        {
            return await table.Where(x => x.User.RoleId == "ADMIN").ToListAsync();
        }

    }
}