using System.ComponentModel.DataAnnotations;

namespace BlogApp.ViewModels
{

    public class CommentCreateViewModel
    {
        [Required]
        public string Body { get; set; }
        public Guid PostId { get; set; }
    }
}
