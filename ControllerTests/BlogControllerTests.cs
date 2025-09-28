using BlogApp.Controllers;
using BlogApp.Models;
using BlogApp.Repositories;
using BlogApp.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

public class BlogControllerTests
{
    private readonly Mock<IBlogRepository> _mockRepo;
    private readonly BlogController _controller;

    public BlogControllerTests()
    {
        _mockRepo = new Mock<IBlogRepository>();
        _controller = new BlogController(_mockRepo.Object);
    }

    [Fact]
    public async Task Index_ReturnsAllBlogs()
    {
        // Arrange
        _mockRepo.Setup(repo => repo.GetAllWithPostsAndOwnerAsync())
            .ReturnsAsync(new List<Blog>
            {
                new Blog { Id = Guid.NewGuid(), Title = "Blog 1", Description = "Desc 1", IsOpen = true },
                new Blog { Id = Guid.NewGuid(), Title = "Blog 2", Description = "Desc 2", IsOpen = false },
            });


        // Act
        var result = await _controller.Index();

        // Assert
        Assert.NotNull(result);
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<BlogApp.ViewModels.BlogViewModel>>(viewResult.Model);
        Assert.Equal(2, model.Count());
    }

    [Fact]
    public async Task Details_ReturnsBlog_WhenFound()
    {
        // Arrange
        var blogId = Guid.NewGuid();
        _mockRepo.Setup(repo => repo.GetBlogWithPostsAndCommentsAsync(blogId))
            .ReturnsAsync(new Blog
            {
                Id = blogId,
                Title = "Blog 1",
                Description = "Desc 1",
                IsOpen = true,
                Posts = new List<Post>()
            });


        // Act
        var result = await _controller.Details(blogId);

        // Assert
        Assert.NotNull(result);
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<BlogViewModel>(viewResult.Model);
        Assert.Equal(blogId, model.Id);
        Assert.Equal("Blog 1", model.Title);
    }

    [Fact]
    public async Task Details_ReturnsNotFound_WhenBlogDoesNotExist()
    {
        // Arrange
        var blogId = Guid.NewGuid();
        _mockRepo.Setup(repo => repo.GetWithPostsAsync(blogId))
            .ReturnsAsync((Blog?)null);

        // Act
        var result = await _controller.Details(blogId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }


    [Fact]
    public async Task Create_Get_ReturnsView()
    {
        // Act
        var result = _controller.Create();
        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Null(viewResult.Model); // No model passed for GET
    }

    [Fact]
    public async Task Create_PostValidModelWithLoggedInUser_RedirectsToDetails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SimulateLoggedInUser(_controller, userId);

        var model = new BlogCreateViewModel
        {
            Title = "Test Blog",
            Description = "Test Desc",
            IsOpen = true
        };

        // Act
        var result = await _controller.Create(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(BlogController.Details), redirectResult.ActionName);
        Assert.NotNull(redirectResult.RouteValues?["id"]);
        Assert.IsType<Guid>(redirectResult.RouteValues?["id"]!);

        _mockRepo.Verify(r => r.AddAsync(It.IsAny<Blog>()), Times.Once);
    }

    [Fact]
    public async Task Edit_Get_ReturnsView_WhenOwner()
    {
        // Arrange
        var blogId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        var blog = new Blog
        {
            Id = blogId,
            Title = "Test Blog",
            Description = "Test Desc",
            IsOpen = true,
            OwnerId = ownerId
        };

        _mockRepo.Setup(r => r.GetAsync(blogId))
            .ReturnsAsync(blog);

        // Simulate logged-in user (the owner)
        SimulateLoggedInUser(_controller, ownerId);

        // Act
        var result = await _controller.Edit(blogId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<BlogEditViewModel>(viewResult.Model);
        Assert.Equal(blogId, model.Id);
        Assert.Equal(blog.Title, model.Title);
        Assert.Equal(blog.Description, model.Description);
        Assert.Equal(blog.IsOpen, model.IsOpen);
    }

    [Fact]
    public async Task Edit_Get_ReturnsForbid_WhenNotOwner()
    {
        // Arrange
        var blogId = Guid.NewGuid();
        var ownerId = Guid.NewGuid(); // actual owner
        var otherUserId = Guid.NewGuid(); // logged-in user

        var blog = new Blog
        {
            Id = blogId,
            Title = "Test Blog",
            Description = "Test Desc",
            IsOpen = true,
            OwnerId = ownerId
        };

        _mockRepo.Setup(r => r.GetAsync(blogId))
            .ReturnsAsync(blog);

        // Simulate logged-in user (not owner)
        SimulateLoggedInUser(_controller, otherUserId);

        // Act
        var result = await _controller.Edit(blogId);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task Edit_Get_ReturnsNotFound_WhenBlogDoesNotExist()
    {
        // Arrange
        var blogId = Guid.NewGuid();

        _mockRepo.Setup(r => r.GetAsync(blogId))
            .ReturnsAsync((Blog?)null);

        // Act
        var result = await _controller.Edit(blogId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_Get_ReturnsView_WhenOwner()
    {
        // Arrange
        var blogId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        var blog = new Blog
        {
            Id = blogId,
            Title = "Test Blog",
            Description = "Test Desc",
            IsOpen = true,
            OwnerId = ownerId
        };

        _mockRepo.Setup(r => r.GetAsync(blogId))
            .ReturnsAsync(blog);

        SimulateLoggedInUser(_controller, ownerId);

        // Act
        var result = await _controller.Delete(blogId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<BlogViewModel>(viewResult.Model);
        Assert.Equal(blogId, model.Id);
    }

    [Fact]
    public async Task Delete_Get_ReturnsForbid_WhenNotOwner()
    {
        var blogId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var blog = new Blog { Id = blogId, OwnerId = ownerId };

        _mockRepo.Setup(r => r.GetAsync(blogId)).ReturnsAsync(blog);

        SimulateLoggedInUser(_controller, otherUserId);

        var result = await _controller.Delete(blogId);

        Assert.IsType<ForbidResult>(result);
    }


    [Fact]
    public async Task Delete_Get_ReturnsNotFound_WhenBlogDoesNotExist()
    {
        var blogId = Guid.NewGuid();

        _mockRepo.Setup(r => r.GetAsync(blogId)).ReturnsAsync((Blog?)null);

        var result = await _controller.Delete(blogId);

        Assert.IsType<NotFoundResult>(result);
    }


    [Fact]
    public async Task DeleteConfirmed_Post_DeletesBlog_WhenOwner()
    {
        var blogId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        var blog = new Blog { Id = blogId, OwnerId = ownerId };

        _mockRepo.Setup(r => r.GetAsync(blogId)).ReturnsAsync(blog);

        SimulateLoggedInUser(_controller, ownerId);

        var result = await _controller.DeleteConfirmed(blogId);

        _mockRepo.Verify(r => r.DeleteAsync(blog), Times.Once);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(BlogController.Index), redirect.ActionName);
    }

    [Fact]
    public async Task DeleteConfirmed_Post_ReturnsForbid_WhenNotOwner()
    {
        var blogId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var blog = new Blog { Id = blogId, OwnerId = ownerId };
        _mockRepo.Setup(r => r.GetAsync(blogId)).ReturnsAsync(blog);

        SimulateLoggedInUser(_controller, otherUserId);

        var result = await _controller.DeleteConfirmed(blogId);

        _mockRepo.Verify(r => r.DeleteAsync(It.IsAny<Blog>()), Times.Never);

        Assert.IsType<ForbidResult>(result);
    }


    private void SimulateLoggedInUser(Controller controller, Guid userId)
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }, "mock"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }


}
