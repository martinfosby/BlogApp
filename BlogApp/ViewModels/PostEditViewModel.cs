using System.ComponentModel.DataAnnotations;

namespace BlogApp.ViewModels
{
    public class PostEditViewModel
    {
        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Body { get; set; }

        public Guid BlogId { get; set; }
    }
}
