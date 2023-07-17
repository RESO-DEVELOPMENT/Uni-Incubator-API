using Application.Domain.Models;

namespace Application.Persistence.Repositories
{
    public class TransactionRepository : BaseRepository<Transaction, Guid>
    {
        public TransactionRepository(DataContext context) : base(context)
        {
        }
    }
}