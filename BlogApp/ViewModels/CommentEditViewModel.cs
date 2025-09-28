using System.ComponentModel.DataAnnotations;

namespace BlogApp.ViewModels
{
    public class CommentEditViewModel
    {
        public Guid Id { get; set; }

        [Required]
        public string Body { get; set; }

        public Guid PostId { get; set; }
    }
}
