using Microsoft.EntityFrameworkCore;

namespace Application.Persistence.Repositories
{
    public class BaseRepository<T, K> where T : class
    {
        private DataContext _context;
        public DbSet<T> table;

        public BaseRepository(DataContext context)
        {
            _context = context;
            table = _context.Set<T>();
        }

        public IQueryable<T> GetQuery()
        {
            return table.AsQueryable();
        }

        public DataContext GetContext()
        {
            return _context;
        }

        public async Task<List<T>> GetAll()
        {
            return await table.ToListAsync();
        }

        public async Task<T?> GetByID(K id)
        {
            return await table.FindAsync(id);
        }

        public void Add(T e)
        {
            table.Add(e);
        }

        public void Add(List<T> e)
        {
            table.AddRange(e);
        }

        public void Update(T e)
        {
            table.Update(e);
        }

        public void Update(List<T> e)
        {
            table.UpdateRange(e);
        }

        public void Delete(T e)
        {
            table.Remove(e);
        }

        public void Delete(List<T> e)
        {
            table.RemoveRange(e);
        }


        // public async Task<bool> SaveAsync()
        // {
        //     var result = await _context.SaveChangesAsync() > 0;
        //     return result;
        // }

        // public bool Save()
        // {
        //     return _context.SaveChanges() > 0;
        // }

        // public IDbContextTransaction BeginTransaction()
        // {
        //     return _context.Database.BeginTransaction();
        // }
    }
}