using Xunit;
using Moq;
using BlogApp.Controllers;
using BlogApp.Repositories;
using BlogApp.Models;
using BlogApp.ViewModels;
using BlogApp.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

public class CommentControllerTests
{
    private readonly Mock<ICommentRepository> _mockCommentRepo;
    private readonly Mock<IPostRepository> _mockPostRepo;
    private readonly Mock<IAuthorizationService> _mockAuthz;
    private readonly CommentController _controller;

    public CommentControllerTests()
    {
        _mockCommentRepo = new Mock<ICommentRepository>();
        _mockPostRepo = new Mock<IPostRepository>();
        _mockAuthz = new Mock<IAuthorizationService>();
        _controller = new CommentController(_mockCommentRepo.Object, _mockPostRepo.Object, _mockAuthz.Object);
    }

    [Fact]
    public async Task ByPost_ReturnsComments()
    {
        // Arrange
        var postId = Guid.NewGuid();
        _mockCommentRepo.Setup(r => r.GetByPostIdAsync(postId))
            .ReturnsAsync(new List<Comment>
            {
                new Comment { Id = Guid.NewGuid(), Body = "Comment 1", PostId = postId },
                new Comment { Id = Guid.NewGuid(), Body = "Comment 2", PostId = postId }
            });


        // Act
        var result = await _controller.ByPost(postId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<CommentViewModel>>(viewResult.Model);
        Assert.Equal(2, model.Count());
    }

    [Fact]
    public async Task Create_PostAddsAndRedirects_WhenValid()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _mockPostRepo.Setup(r => r.GetWithBlogAsync(postId))
            .ReturnsAsync(new Post
            {
                Id = postId,
                Blog = new Blog { Id = Guid.NewGuid(), IsOpen = true }
            });


        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                ], "TestAuth"))
            }
        };

        var model = new CommentCreateViewModel
        {
            PostId = postId,
            Body = "Test comment"
        };

        // Act
        var result = await _controller.Create(model);

        // Assert
        _mockCommentRepo.Verify(r => r.AddAsync(It.IsAny<Comment>()), Times.Once);
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirectResult.ActionName);
        Assert.Equal("Post", redirectResult.ControllerName);
        Assert.Equal(postId, redirectResult.RouteValues["id"]);
    }

    [Fact]
    public async Task Edit_ReturnsForbid_WhenNotOwner()
    {
        // Arrange
        var commentId = Guid.NewGuid();

        var comment = new Comment { Id = commentId, Body = "Hello", OwnerId = Guid.NewGuid() };
        _mockCommentRepo.Setup(r => r.GetAsync(commentId)).ReturnsAsync(comment);


        _mockAuthz.Setup(a => a.AuthorizeAsync(
            It.IsAny<ClaimsPrincipal>(),
            comment,
            It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Failed());


        // Act
        var result = await _controller.Edit(commentId);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task Delete_CallsRepository_WhenAuthorized()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var comment = new Comment { Id = commentId, PostId = Guid.NewGuid(), OwnerId = userId };
        _mockCommentRepo.Setup(r => r.GetAsync(commentId)).ReturnsAsync(comment);

        _mockAuthz.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), comment, It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                 .ReturnsAsync(AuthorizationResult.Success());

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                }, "TestAuth"))
            }
        };

        // Act
        var result = await _controller.Delete(commentId);

        // Assert
        _mockCommentRepo.Verify(r => r.DeleteAsync(comment), Times.Once);
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirectResult.ActionName);
        Assert.Equal("Post", redirectResult.ControllerName);
    }
}
