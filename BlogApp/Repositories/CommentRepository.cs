using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BlogApp.Data;
using BlogApp.Models;


namespace BlogApp.Repositories
{
    public interface ICommentRepository : IRepository<Comment>
    {
        Task<IEnumerable<Comment>> GetByPostIdAsync(Guid postId);
    }


    public class CommentRepository : Repository<Comment>, ICommentRepository
    {
        public CommentRepository(BlogAppContext db) : base(db) { }


        public async Task<IEnumerable<Comment>> GetByPostIdAsync(Guid postId)
        {
            return await _set.Include(c => c.Owner).Where(c => c.PostId == postId).ToListAsync();
        }
    }
}