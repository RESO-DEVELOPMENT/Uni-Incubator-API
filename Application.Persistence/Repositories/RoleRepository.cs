using Application.Domain.Models;

namespace Application.Persistence.Repositories
{
    public class RoleRepository : BaseRepository<Role, String>
    {
        public RoleRepository(DataContext context) : base(context)
        {
        }
    }
}