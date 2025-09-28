using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace BlogApp.Models
{
    public class Blog
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsOpen { get; set; } = true; // true = open for new posts/comments
        public Guid OwnerId { get; set; }
        public User Owner { get; set; }


        public ICollection<Post> Posts { get; set; }
    }
}