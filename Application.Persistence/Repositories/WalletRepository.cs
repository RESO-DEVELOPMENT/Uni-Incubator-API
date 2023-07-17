using Application.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Application.Persistence.Repositories
{
  public class WalletRepository : BaseRepository<Wallet, Guid>
  {
    public WalletRepository(DataContext context) : base(context)
    {
    }

    public async Task<List<Wallet>> GetSystemWallets()
    {
      return await table.Where(x => x.IsSystem).ToListAsync();
    }
  }
}