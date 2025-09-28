using BlogApp.Models;

namespace BlogApp.ViewModels
{
    public class BlogViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsOpen { get; set; }
        public Guid OwnerId { get; set; }
        public User Owner { get; set; }
        public IEnumerable<Post> Posts { get; set; }
        public string? CurrentUserId { get; set; }
    }
}
