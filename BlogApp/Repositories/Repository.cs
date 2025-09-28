using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BlogApp.Data;


namespace BlogApp.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly BlogAppContext _db;
        protected readonly DbSet<T> _set;
        public Repository(BlogAppContext db)
        {
            _db = db;
            _set = db.Set<T>();
        }


        public virtual async Task AddAsync(T entity)
        {
            await _set.AddAsync(entity);
            await _db.SaveChangesAsync();
        }


        public virtual async Task DeleteAsync(T entity)
        {
            _set.Remove(entity);
            await _db.SaveChangesAsync();
        }


        public virtual async Task<T> GetAsync(Guid id)
        {
            return await _set.FindAsync(id);
        }


        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _set.ToListAsync();
        }


        public virtual async Task UpdateAsync(T entity)
        {
            _set.Update(entity);
            await _db.SaveChangesAsync();
        }
    }
}