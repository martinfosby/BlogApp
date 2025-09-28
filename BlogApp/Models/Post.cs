using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;


namespace BlogApp.Models
{
    public class Post
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Title { get; set; }
        public string Body { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public Guid BlogId { get; set; }
        public Blog Blog { get; set; }


        public Guid OwnerId { get; set; }
        public User Owner { get; set; }


        public ICollection<Comment> Comments { get; set; }
    }
}