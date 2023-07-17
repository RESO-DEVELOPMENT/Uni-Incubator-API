using Attribute = Application.Domain.Models.Attribute;

namespace Application.Persistence.Repositories
{
    public class AttributeRepository : BaseRepository<Attribute, int>
    {
        public AttributeRepository(DataContext context) : base(context)
        {
        }
    }
}