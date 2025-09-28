using BlogApp.Authorization;
using BlogApp.Models;
using BlogApp.Repositories;
using BlogApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlogApp.Controllers
{
    [Authorize]
    public class CommentController : Controller
    {
        private readonly ICommentRepository _comments;
        private readonly IPostRepository _posts;
        private readonly IAuthorizationService _authz;

        public CommentController(ICommentRepository comments, IPostRepository posts, IAuthorizationService authz)
        {
            _comments = comments;
            _posts = posts;
            _authz = authz;
        }

        // GET: /Comment/Post/{postId}
        [AllowAnonymous]
        public async Task<IActionResult> ByPost(Guid postId)
        {
            var comments = await _comments.GetByPostIdAsync(postId);
            var viewModels = comments.Select(c => new CommentViewModel
            {
                Id = c.Id,
                Body = c.Body,
                CreatedAt = c.CreatedAt,
                OwnerId = c.OwnerId,
                PostId = c.PostId
            });
            return View(viewModels);
        }

        // GET: /Comment/Create?postId=xxx
        public IActionResult Create(Guid postId)
        {
            return View(new CommentCreateViewModel { PostId = postId });
        }

        // POST: /Comment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CommentCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var post = await _posts.GetWithBlogAsync(model.PostId);
            if (post == null) return NotFound();
            if (!post.Blog.IsOpen) return Forbid();

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                Body = model.Body,
                PostId = model.PostId,
                OwnerId = userId
            };

            await _comments.AddAsync(comment);

            TempData["SuccessMessage"] = "Comment added successfully.";
            return RedirectToAction("Details", "Blog", new { id = post.BlogId });
        }

        // GET: /Comment/Edit/{id}
        public async Task<IActionResult> Edit(Guid id)
        {
            var comment = await _comments.GetAsync(id);
            if (comment == null) return NotFound();

            var authResult = await _authz.AuthorizeAsync(User, comment, new OwnershipRequirement());
            if (!authResult.Succeeded) return Forbid();

            var model = new CommentEditViewModel
            {
                Id = comment.Id,
                Body = comment.Body,
                PostId = comment.PostId
            };
            return View(model);
        }

        // POST: /Comment/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CommentEditViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var comment = await _comments.GetAsync(model.Id);
            if (comment == null) return NotFound();

            var authResult = await _authz.AuthorizeAsync(User, comment, new OwnershipRequirement());
            if (!authResult.Succeeded) return Forbid();

            comment.Body = model.Body;
            await _comments.UpdateAsync(comment);

            TempData["SuccessMessage"] = "Comment updated successfully.";
            return RedirectToAction("Details", "Post", new { id = model.PostId });
        }

        // POST: /Comment/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid commentId)
        {
            var comment = await _comments.GetCommentWithPost(commentId);
            if (comment == null) return NotFound();

            var authResult = await _authz.AuthorizeAsync(User, comment, new OwnershipRequirement());
            if (!authResult.Succeeded) return Forbid();

            await _comments.DeleteAsync(comment);

            TempData["SuccessMessage"] = "Comment deleted successfully.";
            return RedirectToAction("Details", "Blog", new { id = comment.Post.BlogId });
        }
    }
}
