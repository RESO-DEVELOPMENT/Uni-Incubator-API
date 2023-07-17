using Application.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Application.Persistence.Repositories
{
    public class UserRepository : BaseRepository<User, Guid>
    {
        public UserRepository(DataContext context) : base(context)
        {
        }

        public async Task<User?> GetByEmail(string email)
        {
            return await table.FirstOrDefaultAsync(x => x.EmailAddress == email);
        }

        public async Task<User?> GetByPasswordResetToken(string token)
        {
            return await table.FirstOrDefaultAsync(x => x.PasswordChangeToken == token);
        }


        public async Task<User?> GetByEmailWithMember(string email)
        {
            return await table
                .Include(x => x.Member)
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.EmailAddress == email);
        }
    }
}