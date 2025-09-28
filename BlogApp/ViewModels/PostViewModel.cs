using BlogApp.Models;

namespace BlogApp.ViewModels
{
    public class PostViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid OwnerId { get; set; }
        public User Owner { get; internal set; }
        public Guid BlogId { get; set; }
        public bool IsOpen { get; set; }
        public IEnumerable<Comment> Comments { get; set; } // optional
    }
}
