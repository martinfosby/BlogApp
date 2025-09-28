using Xunit;
using Moq;
using BlogApp.Controllers;
using BlogApp.Repositories;
using BlogApp.Models;
using BlogApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

public class HomeControllerTests
{
    private readonly Mock<IBlogRepository> _mockRepo;
    private readonly Mock<Microsoft.Extensions.Logging.ILogger<HomeController>> _mockLogger;
    private readonly HomeController _controller;
    private readonly Guid _userId;

    public HomeControllerTests()
    {
        // Arrange common dependencies
        _userId = Guid.NewGuid();

        _mockRepo = new Mock<IBlogRepository>();
        _mockRepo.Setup(r => r.GetAllWithPostsAsync())
            .ReturnsAsync(new List<Blog>
            {
                new Blog { Id = Guid.NewGuid(), Title = "Blog 1", OwnerId = _userId },
                new Blog { Id = Guid.NewGuid(), Title = "Blog 2", OwnerId = Guid.NewGuid() } // another user
            });

        _mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<HomeController>>();

        _controller = new HomeController(_mockLogger.Object, _mockRepo.Object);
    }

    private void AuthenticateController()
    {
        // Set up a fake authenticated user
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, _userId.ToString())
                }, "TestAuth"))
            }
        };
    }

    private void UnauthenticatedController()
    {
        // No user set (unauthenticated)
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext()
        };
    }

    [Fact]
    public async Task Index_ReturnsUserBlogs_WhenAuthenticated()
    {
        AuthenticateController();

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<BlogViewModel>>(viewResult.Model);
        Assert.Single(model); // only 1 blog belongs to this user
        Assert.Equal("Blog 1", model.First().Title);
    }

    [Fact]
    public async Task Index_ReturnsEmpty_WhenNotAuthenticated()
    {
        UnauthenticatedController();

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<BlogViewModel>>(viewResult.Model);
        Assert.Empty(model); // no blogs
    }
}
