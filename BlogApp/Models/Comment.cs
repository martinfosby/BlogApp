using System;
using System.ComponentModel.DataAnnotations;


namespace BlogApp.Models
{
    public class Comment
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Body { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public Guid PostId { get; set; }
        public Post Post { get; set; }


        public Guid OwnerId { get; set; }
        public User Owner { get; set; }
    }
}