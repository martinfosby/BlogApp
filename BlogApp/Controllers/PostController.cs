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
    public class PostController : Controller
    {
        private readonly IPostRepository _posts;
        private readonly IBlogRepository _blogs;
        private readonly IAuthorizationService _authz;

        public PostController(IPostRepository posts, IBlogRepository blogs, IAuthorizationService authz)
        {
            _posts = posts;
            _blogs = blogs;
            _authz = authz;
        }

        // GET: /Post/ByBlog/{blogId}
        [AllowAnonymous]
        public async Task<IActionResult> ByBlog(Guid blogId)
        {
            var posts = await _posts.GetByBlogIdAsync(blogId);
            var viewModels = posts.Select(p => new PostViewModel
            {
                Id = p.Id,
                Title = p.Title,
                Body = p.Body,
                CreatedAt = p.CreatedAt,
                OwnerId = p.OwnerId,
                BlogId = p.BlogId
            });
            return View(viewModels);
        }

        // GET: /Post/Details/{id}
        [AllowAnonymous]
        public async Task<IActionResult> Details(Guid id)
        {
            var post = await _posts.GetWithBlogAndCommentsAndOwnerAsync(id);
            if (post == null) return NotFound();

            var viewModel = new PostViewModel
            {
                Id = post.Id,
                Title = post.Title,
                Body = post.Body,
                CreatedAt = post.CreatedAt,
                OwnerId = post.OwnerId,
                Owner = post.Owner,
                BlogId = post.BlogId,
                IsOpen = post.Blog.IsOpen,
                Comments = post.Comments
            };

            return View(viewModel);
        }

        // GET: /Post/Create?blogId=xxx
        public IActionResult Create(Guid blogId)
        {
            return View(new PostCreateViewModel { BlogId = blogId });
        }

        // POST: /Post/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PostCreateViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var blog = await _blogs.GetAsync(model.BlogId);
            if (blog == null) return NotFound();
            if (!blog.IsOpen) return Forbid();

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var post = new Post
            {
                Id = Guid.NewGuid(),
                Title = model.Title,
                Body = model.Body,
                BlogId = model.BlogId,
                OwnerId = userId
            };

            await _posts.AddAsync(post);

            TempData["SuccessMessage"] = "Post created successfully.";
            return RedirectToAction("Details", new { id = post.Id });
        }

        // GET: /Post/Edit/{id}
        public async Task<IActionResult> Edit(Guid id)
        {
            var post = await _posts.GetAsync(id);
            if (post == null) return NotFound();

            var authResult = await _authz.AuthorizeAsync(User, post, new OwnershipRequirement());
            if (!authResult.Succeeded) return Forbid();

            var model = new PostEditViewModel
            {
                Id = post.Id,
                Title = post.Title,
                Body = post.Body,
                BlogId = post.BlogId
            };

            return View(model);
        }

        // POST: /Post/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PostEditViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var post = await _posts.GetAsync(model.Id);
            if (post == null) return NotFound();

            var authResult = await _authz.AuthorizeAsync(User, post, new OwnershipRequirement());
            if (!authResult.Succeeded) return Forbid();

            post.Title = model.Title;
            post.Body = model.Body;
            await _posts.UpdateAsync(post);

            TempData["SuccessMessage"] = "Post updated successfully.";
            return RedirectToAction("Details", new { id = post.Id });
        }

        // POST: /Post/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var post = await _posts.GetAsync(id);
            if (post == null) return NotFound();

            var authResult = await _authz.AuthorizeAsync(User, post, new OwnershipRequirement());
            if (!authResult.Succeeded) return Forbid();

            await _posts.DeleteAsync(post);
            TempData["SuccessMessage"] = "Post deleted successfully.";
            return RedirectToAction("ByBlog", new { blogId = post.BlogId });
        }
    }
}
