using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BlogApp.Models;

namespace BlogApp.Data
{
    public class BlogAppContext(DbContextOptions<BlogAppContext> options) : DbContext(options)
    {
        public DbSet<BlogApp.Models.User> User { get; set; } = default!;
        public DbSet<BlogApp.Models.Blog> Blog { get; set; } = default!;
        public DbSet<BlogApp.Models.Post> Post { get; set; } = default!;
        public DbSet<BlogApp.Models.Comment> Comment { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Blog>().ToTable("blogs");
            modelBuilder.Entity<Post>().ToTable("posts");
            modelBuilder.Entity<Comment>().ToTable("comments");


            // SEEDING

            // seed some users
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Username = "alice",
                    PasswordHash = "hashedpassword1"
                },
                new User
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Username = "bob",
                    PasswordHash = "hashedpassword2"
                }
            );

            // seed some blogs
            modelBuilder.Entity<Blog>().HasData(
                new Blog
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    Title = "Alice's Adventures",
                    Description = "A blog about Alice's adventures.",
                    IsOpen = true,
                    OwnerId = Guid.Parse("11111111-1111-1111-1111-111111111111") // Alice
                },
                new Blog
                {
                    Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                    Title = "Bob's Thoughts",
                    Description = "A blog about Bob's thoughts.",
                    IsOpen = true,
                    OwnerId = Guid.Parse("22222222-2222-2222-2222-222222222222") // Bob
                }
            );

            // seed some posts
            modelBuilder.Entity<Post>().HasData(
                new Post
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Title = "Alice's First Post",
                    Body = "This is the body of Alice's first post.",
                    CreatedAt = DateTime.UtcNow,
                    BlogId = Guid.Parse("44444444-4444-4444-4444-444444444444"), // Alice's Blog
                    OwnerId = Guid.Parse("11111111-1111-1111-1111-111111111111") // Alice
                },
                new Post
                {
                    Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                    Title = "Bob's First Post",
                    Body = "This is the body of Bob's first post.",
                    CreatedAt = DateTime.UtcNow,
                    BlogId = Guid.Parse("66666666-6666-6666-6666-666666666666"), // Bob's Blog
                    OwnerId = Guid.Parse("22222222-2222-2222-2222-222222222222") // Bob
                }
            );

            // seed some comments
            modelBuilder.Entity<Comment>().HasData(
                new Comment
                {
                    Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                    Body = "Great post, Alice!",
                    CreatedAt = DateTime.UtcNow,
                    PostId = Guid.Parse("33333333-3333-3333-3333-333333333333"), // Alice's Post
                    OwnerId = Guid.Parse("22222222-2222-2222-2222-222222222222") // Bob
                },
                new Comment
                {
                    Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                    Body = "Thanks for sharing, Bob!",
                    CreatedAt = DateTime.UtcNow,
                    PostId = Guid.Parse("55555555-5555-5555-5555-555555555555"), // Bob's Post
                    OwnerId = Guid.Parse("11111111-1111-1111-1111-111111111111") // Alice
                }
            );

            // cascade delete: when a Blog is deleted, delete its Posts and those Posts' Comments
            modelBuilder.Entity<Blog>()
            .HasMany(b => b.Posts)
            .WithOne(p => p.Blog)
            .HasForeignKey(p => p.BlogId)
            .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Post>()
            .HasMany(p => p.Comments)
            .WithOne(c => c.Post)
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
