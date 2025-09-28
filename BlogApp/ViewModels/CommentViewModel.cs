namespace BlogApp.ViewModels
{
    public class CommentViewModel
    {
        public Guid Id { get; set; }
        public string Body { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid OwnerId { get; set; }
        public Guid PostId { get; set; }
    }

}
