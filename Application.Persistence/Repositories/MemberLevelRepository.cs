using Application.Domain.Models;

namespace Application.Persistence.Repositories
{
    public class MemberLevelRepository : BaseRepository<MemberLevel, Guid>
    {
        public MemberLevelRepository(DataContext context) : base(context)
        {
        }
    }
}