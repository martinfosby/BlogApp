using BlogApp.Data;
using BlogApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace BlogApp.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User> GetByUsernameAsync(string username);
    }

    public class UserRepository : IUserRepository
    {
        private readonly BlogAppContext _context;

        public UserRepository(BlogAppContext context)
        {
            _context = context;
        }

        public async Task AddAsync(User entity)
        {
            await _context.User.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(User entity)
        {
            _context.User.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<User> GetAsync(Guid id)
        {
            return await _context.User.FindAsync(id);
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.User.ToListAsync();
        }

        public async Task UpdateAsync(User entity)
        {
            _context.User.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            return await _context.User.FirstOrDefaultAsync(u => u.Username == username);
        }
    }
}
