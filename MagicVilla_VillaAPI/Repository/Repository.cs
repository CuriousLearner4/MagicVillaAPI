using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System.Linq.Expressions;

namespace MagicVilla_VillaAPI.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly AppDbContext db;
        internal DbSet<T> dbset;
        public Repository(AppDbContext db)
        {
            this.db = db;
            this.dbset = db.Set<T>();
        }
        public async Task CreateAsync(T entity)
        {
            await dbset.AddAsync(entity);
            await Save();
        }

        public async Task<T> GetAsync(Expression<Func<T, bool>> filter = null, bool tracked = true)
        {
            IQueryable<T> query = dbset;
            if (!tracked) query = query.AsNoTracking();
            if (filter != null) query = query.Where(filter);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null)
        {
            IQueryable<T> query = dbset;
            if (filter != null) query = query.Where(filter);
            return await query.ToListAsync();

        }

        public async Task RemoveAsync(T entity)
        {
            dbset.Remove(entity);
            await Save();
        }

        public async Task Save()
        {
            await db.SaveChangesAsync();

        }

        public async Task UpdateAsync(T entity)
        {
            dbset.Update(entity);
            await Save();
        }
    }
}
