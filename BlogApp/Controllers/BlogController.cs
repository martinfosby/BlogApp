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
    public class BlogController : Controller
    {
        private readonly IBlogRepository _blogs;
        public BlogController(IBlogRepository blogs)
        {
            _blogs = blogs;
        }

        // GET: /Blog
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var all = await _blogs.GetAllWithPostsAndOwnerAsync();
            var viewModels = all.Select(b => new BlogViewModel
            {
                Id = b.Id,
                Title = b.Title,
                Description = b.Description,
                IsOpen = b.IsOpen,
                OwnerId = b.OwnerId,
                Owner = b.Owner,
            });
            return View(viewModels);
        }

        // GET: /Blog/Details/{id}
        [AllowAnonymous]
        public async Task<IActionResult> Details(Guid id)
        {
            var blog = await _blogs.GetBlogWithPostsAndCommentsAsync(id);
            if (blog == null) return NotFound();

            var currentUserId = User?.FindFirstValue(ClaimTypes.NameIdentifier);

            var viewModel = new BlogViewModel
            {
                Id = blog.Id,
                Title = blog.Title,
                Description = blog.Description,
                IsOpen = blog.IsOpen,
                OwnerId = blog.OwnerId,
                Posts = blog.Posts,
                CurrentUserId = currentUserId
            };
            return View(viewModel);
        }

        // GET: /Blog/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Blog/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BlogCreateViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            {
                // Optionally handle the error, e.g. redirect to login or show an error page
                return Unauthorized();
            }
            var userId = Guid.Parse(userIdClaim.Value);

            var blog = new Blog
            {
                Id = Guid.NewGuid(),
                Title = model.Title,
                Description = model.Description,
                IsOpen = model.IsOpen,
                OwnerId = userId
            };

            await _blogs.AddAsync(blog);

            TempData["SuccessMessage"] = "Blog created successfully.";
            // redirect to the blog details page
            return RedirectToAction(nameof(Details), new { id = blog.Id });
        }

        // GET: /Blog/Edit/{id}
        public async Task<IActionResult> Edit(Guid id)
        {
            var blog = await _blogs.GetAsync(id);
            if (blog == null) return NotFound();

            var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || blog.OwnerId != Guid.Parse(userIdClaim.Value))
            {
                return Forbid();
            }

            var viewModel = new BlogEditViewModel
            {
                Id = blog.Id,
                Title = blog.Title,
                Description = blog.Description,
                IsOpen = blog.IsOpen
            };

            return View(viewModel);
        }

        // POST: /Blog/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, BlogEditViewModel model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            var blog = await _blogs.GetAsync(id);
            if (blog == null) return NotFound();

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || blog.OwnerId != Guid.Parse(userIdClaim.Value))
            {
                return Forbid();
            }

            blog.Title = model.Title;
            blog.Description = model.Description;
            blog.IsOpen = model.IsOpen;

            await _blogs.UpdateAsync(blog);

            TempData["SuccessMessage"] = "Blog updated successfully.";
            return RedirectToAction(nameof(Details), new { id = blog.Id });
        }

        // GET: /Blog/Delete/{id}
        public async Task<IActionResult> Delete(Guid id)
        {
            var blog = await _blogs.GetAsync(id);
            if (blog == null) return NotFound();

            var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || blog.OwnerId != Guid.Parse(userIdClaim.Value))
            {
                return Forbid(); // only owner can delete
            }

            var viewModel = new BlogViewModel
            {
                Id = blog.Id,
                Title = blog.Title,
                Description = blog.Description,
                IsOpen = blog.IsOpen,
                OwnerId = blog.OwnerId
            };

            return View(viewModel); // confirmation view
        }

        // POST: /Blog/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var blog = await _blogs.GetAsync(id);
            if (blog == null) return NotFound();

            var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || blog.OwnerId != Guid.Parse(userIdClaim.Value))
            {
                return Forbid(); // only owner can delete
            }

            await _blogs.DeleteAsync(blog); // repository should handle cascade delete for posts/comments

            TempData["SuccessMessage"] = "Blog deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

    }
}
