using System.ComponentModel.DataAnnotations;

namespace BlogApp.ViewModels
{
    public class BlogCreateViewModel
    {
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsOpen { get; set; }
    }
}
