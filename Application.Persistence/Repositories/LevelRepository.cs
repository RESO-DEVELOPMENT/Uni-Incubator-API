using Application.Domain.Enums.Level;
using Application.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Application.Persistence.Repositories
{
    public class LevelRepository : BaseRepository<Level, int>
    {
        public LevelRepository(DataContext context) : base(context)
        {
        }

        public async Task<Level> GetFirstLevel()
        {
            return await table.OrderBy(x => x.XPNeeded).Where(x => x.Status == LevelStatus.Active).FirstAsync();
        }
    }
}