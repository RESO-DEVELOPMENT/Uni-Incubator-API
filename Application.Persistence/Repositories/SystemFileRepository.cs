using Application.Domain.Models;

namespace Application.Persistence.Repositories
{
    public class SystemFileRepository : BaseRepository<SystemFile, Guid>
    {
        public SystemFileRepository(DataContext context) : base(context)
        {
        }
    }
}