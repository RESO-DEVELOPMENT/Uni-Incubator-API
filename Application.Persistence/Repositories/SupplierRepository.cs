using Application.Domain.Models;

namespace Application.Persistence.Repositories
{
    public class SupplierRepository : BaseRepository<Supplier, Guid>
    {
        public SupplierRepository(DataContext context) : base(context)
        {
        }
    }
}