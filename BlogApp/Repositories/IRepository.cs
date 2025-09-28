using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace BlogApp.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<T> GetAsync(Guid id);
        Task<IEnumerable<T>> GetAllAsync();
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
    }
}