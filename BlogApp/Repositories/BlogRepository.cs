using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BlogApp.Data;
using BlogApp.Models;


namespace BlogApp.Repositories
{
    public interface IBlogRepository : IRepository<Blog>
    {
        Task<IEnumerable<Blog>> GetAllWithPostsAsync();
        Task<IEnumerable<Blog>> GetAllWithPostsAndOwnerAsync();

        Task<Blog> GetWithPostsAsync(Guid id);
        Task<Blog?> GetBlogWithPostsAndCommentsAsync(Guid id);

    }


    public class BlogRepository : Repository<Blog>, IBlogRepository
    {
        public BlogRepository(BlogAppContext db) : base(db) { }


        public async Task<IEnumerable<Blog>> GetAllWithPostsAsync()
        {
            return await _set.Include(b => b.Posts).ToListAsync();
        }

        public async Task<IEnumerable<Blog>> GetAllWithPostsAndOwnerAsync()
        {
            return await _set.Include(b => b.Posts).Include(b => b.Owner).ToListAsync();
        }


        public async Task<Blog> GetWithPostsAsync(Guid id)
        {
            return await _set.Include(b => b.Posts).ThenInclude(p => p.Comments).FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<Blog?> GetBlogWithPostsAndCommentsAsync(Guid id)
        {
            return await _set
                .Include(b => b.Owner) // blog owner
                .Include(b => b.Posts)
                    .ThenInclude(p => p.Owner) // post owner
                .Include(b => b.Posts)
                    .ThenInclude(p => p.Comments)
                        .ThenInclude(c => c.Owner) // comment owner
                .FirstOrDefaultAsync(b => b.Id == id);
        }

    }
}