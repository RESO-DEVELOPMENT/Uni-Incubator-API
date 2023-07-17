using Application.Domain.Models;

namespace Application.Persistence.Repositories
{
    public class AttributeGroupRepository : BaseRepository<AttributeGroup, int>
    {
        public AttributeGroupRepository(DataContext context) : base(context)
        {
        }
    }
}