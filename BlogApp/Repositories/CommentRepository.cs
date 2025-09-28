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
        Task<IEnumerable<Comment>> GetAllCommentsWithBlog();
        Task<Comment> GetCommentWithPostAndBlog(Guid commentId);
        Task<Comment> GetCommentWithPost(Guid commentId);


    }


    public class CommentRepository : Repository<Comment>, ICommentRepository
    {
        public CommentRepository(BlogAppContext db) : base(db) { }


        public async Task<IEnumerable<Comment>> GetByPostIdAsync(Guid postId)
        {
            return await _set.Include(c => c.Owner).Where(c => c.PostId == postId).ToListAsync();
        }

        public async Task<IEnumerable<Comment>> GetAllCommentsWithBlog()
        {
            return await _set.Include(c => c.Post).ThenInclude(p => p.Blog).ToListAsync();
        }

        public async Task<Comment> GetCommentWithPostAndBlog(Guid commentId)
        {
            return await _set.Include(c => c.Post).ThenInclude(p => p.Blog).FirstOrDefaultAsync(c => c.Id == commentId);
        }

        public async Task<Comment> GetCommentWithPost(Guid commentId)
        {
            return await _set.Include(c => c.Post).FirstOrDefaultAsync(c => c.Id == commentId);
        }
    }
}