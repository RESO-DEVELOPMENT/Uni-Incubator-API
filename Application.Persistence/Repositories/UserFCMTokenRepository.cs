using Application.Domain.Models;

namespace Application.Persistence.Repositories
{
    public class UserFCMTokenRepository : BaseRepository<UserFCMToken, Guid>
    {
        public UserFCMTokenRepository(DataContext context) : base(context)
        {
        }
    }
}