using Xunit;
using Moq;
using BlogApp.Controllers;
using BlogApp.Repositories;
using BlogApp.Models;
using BlogApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Linq;

public class PostControllerTests : ControllerTestBase<PostController>
{
    private readonly Mock<IPostRepository> _mockPostRepo;
    private readonly Mock<IBlogRepository> _mockBlogRepo;
    private readonly Mock<IAuthorizationService> _mockAuthz;
    private readonly PostController _controller;

    public PostControllerTests()
    {
        _mockPostRepo = new Mock<IPostRepository>();
        _mockBlogRepo = new Mock<IBlogRepository>();
        _mockAuthz = new Mock<IAuthorizationService>();
        _controller = new PostController(_mockPostRepo.Object, _mockBlogRepo.Object, _mockAuthz.Object);
        InitializeTempData(_controller);
    }

    [Fact]
    public async Task ByBlog_ReturnsAllPosts()
    {
        // Arrange
        var blogId = Guid.NewGuid();
        _mockPostRepo.Setup(r => r.GetByBlogIdAsync(blogId))
            .ReturnsAsync(new List<Post>
            {
                new Post { Id = Guid.NewGuid(), Title = "Post 1", Body = "Body 1", BlogId = blogId },
                new Post { Id = Guid.NewGuid(), Title = "Post 2", Body = "Body 2", BlogId = blogId }
            });

        // Act
        var result = await _controller.ByBlog(blogId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<PostViewModel>>(viewResult.Model);
        Assert.Equal(2, model.Count());
    }

    [Fact]
    public async Task Details_ReturnsPost_WhenFound()
    {
        // Arrange
        var postId = Guid.NewGuid();
        _mockPostRepo.Setup(r => r.GetWithBlogAndCommentsAndOwnerAsync(postId))
            .ReturnsAsync(new Post
            {
                Id = postId,
                Title = "Post 1",
                Body = "Body 1",
                BlogId = Guid.NewGuid(),
                Blog = new Blog { Id = Guid.NewGuid(), IsOpen = true },
                Comments = new List<Comment>()
            });

        // Act
        var result = await _controller.Details(postId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<PostViewModel>(viewResult.Model);
        Assert.Equal(postId, model.Id);
        Assert.True(model.IsOpen);
    }

    [Fact]
    public async Task Details_ReturnsNotFound_WhenPostDoesNotExist()
    {
        // Arrange
        var postId = Guid.NewGuid();
        _mockPostRepo.Setup(r => r.GetWithBlogAndCommentsAndOwnerAsync(postId))
            .ReturnsAsync((Post)null);

        // Act
        var result = await _controller.Details(postId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_PostAddsAndRedirects_WhenValid()
    {
        // Arrange
        var blogId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _mockBlogRepo.Setup(r => r.GetAsync(blogId))
            .ReturnsAsync(new Blog { Id = blogId, IsOpen = true });

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                }, "TestAuth"))
            }
        };

        var model = new PostCreateViewModel
        {
            BlogId = blogId,
            Title = "New Post",
            Body = "Some content"
        };

        // Act
        var result = await _controller.Create(model);

        // Assert
        _mockPostRepo.Verify(r => r.AddAsync(It.IsAny<Post>()), Times.Once);
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirectResult.ActionName);
    }
}
