using BlogApp.Models;
using BlogApp.Repositories;
using BlogApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlogApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBlogRepository _blogs;

        public HomeController(ILogger<HomeController> logger, IBlogRepository blogs)
        {
            _logger = logger;
            _blogs = blogs;
        }

        public async Task<IActionResult> Index()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                // Get user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    _logger.LogWarning(
                        "Authenticated user accessed Index without a NameIdentifier claim at {Time}",
                        DateTime.UtcNow
                    );
                    return View(Enumerable.Empty<BlogViewModel>());
                }

                var userId = Guid.Parse(userIdClaim.Value);
                _logger.LogInformation(
                    "Authenticated user {UserId} accessed Index at {Time}",
                    userId, DateTime.UtcNow
                );

                // Get all blogs
                var userBlogs = await _blogs.GetAllWithPostsAsync();
                var myBlogs = userBlogs
                    .Where(b => b.OwnerId == userId)
                    .Select(b => new BlogViewModel
                    {
                        Id = b.Id,
                        Title = b.Title,
                        Description = b.Description,
                        IsOpen = b.IsOpen,
                        OwnerId = b.OwnerId
                    })
                    .ToList();

                _logger.LogInformation(
                    "{Count} blogs returned for user {UserId} at {Time}",
                    myBlogs.Count, userId, DateTime.UtcNow
                );


                return View(myBlogs);
            }

            // Unauthenticated user
            _logger.LogInformation(
                "Unauthenticated user accessed Index at {Time}",
                DateTime.UtcNow
            );

            return View(Enumerable.Empty<BlogViewModel>());
        }

    }
}
