using BlogApp.Models;
using System.ComponentModel.DataAnnotations;

namespace BlogApp.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }


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

    public class BlogCreateViewModel
    {
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsOpen { get; set; }
    }

    public class BlogEditViewModel
    {
        public Guid Id { get; set; }

        [Required, StringLength(100)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public bool IsOpen { get; set; }
    }


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

    public class PostCreateViewModel
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Body { get; set; }

        public Guid BlogId { get; set; }
    }

    public class PostEditViewModel
    {
        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Body { get; set; }

        public Guid BlogId { get; set; }
    }

    public class CommentViewModel
    {
        public Guid Id { get; set; }
        public string Body { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid OwnerId { get; set; }
        public Guid PostId { get; set; }
    }

    public class CommentCreateViewModel
    {
        [Required]
        public string Body { get; set; }
        public Guid PostId { get; set; }
    }

    public class CommentEditViewModel
    {
        public Guid Id { get; set; }

        [Required]
        public string Body { get; set; }

        public Guid PostId { get; set; }
    }




}
