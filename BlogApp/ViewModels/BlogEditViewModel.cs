using System.ComponentModel.DataAnnotations;

namespace BlogApp.ViewModels
{

    public class BlogEditViewModel
    {
        public Guid Id { get; set; }

        [Required, StringLength(100)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public bool IsOpen { get; set; }
    }

}
