using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BlogApp.Data;
using BlogApp.Models;


namespace BlogApp.Repositories
{
    public interface IPostRepository : IRepository<Post>
    {
        Task<IEnumerable<Post>> GetByBlogIdAsync(Guid blogId);
        Task<Post> GetWithCommentsAsync(Guid id);
        Task<Post> GetWithBlogAsync(Guid id);
        Task<Post> GetWithBlogAndCommentsAndOwnerAsync(Guid id);

    }


    public class PostRepository : Repository<Post>, IPostRepository
    {
        public PostRepository(BlogAppContext db) : base(db) { }

        public async Task<Post> GetWithBlogAsync(Guid id)
        {
            return await _set
                .Include(p => p.Blog)   // <-- include the related blog
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Post> GetWithBlogAndCommentsAndOwnerAsync(Guid id)
        {
            return await _set
                .Include(p => p.Blog)
                .Include(p => p.Comments)
                .Include(p => p.Owner)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Post>> GetByBlogIdAsync(Guid blogId)
        {
            return await _set.Include(p => p.Owner).Where(p => p.BlogId == blogId).ToListAsync();
        }


        public async Task<Post> GetWithCommentsAsync(Guid id)
        {
            return await _set.Include(p => p.Comments).ThenInclude(c => c.Owner).FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}