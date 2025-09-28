using BlogApp.Models;
using BlogApp.Repositories;
using BlogApp.Services;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

public class AuthServiceTests
{
    private Mock<IUserRepository> _userRepoMock;
    private AuthService _authService;

    public AuthServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        var config = new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build();
        _authService = new AuthService(_userRepoMock.Object, config);
    }

    [Fact]
    public async Task RegisterAsync_ShouldCreateUser()
    {
        _userRepoMock.Setup(r => r.GetByUsernameAsync("testuser"))
                     .ReturnsAsync((User)null); // user does not exist

        _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>()))
                     .Returns(Task.CompletedTask);

        var user = await _authService.RegisterAsync("testuser", "password123");

        Assert.NotNull(user);
        Assert.Equal("testuser", user.Username);
        _userRepoMock.Verify(r => r.AddAsync(It.Is<User>(u => u.Username == "testuser")), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrow_WhenUserExists()
    {
        _userRepoMock.Setup(r => r.GetByUsernameAsync("testuser"))
                     .ReturnsAsync(new User { Username = "testuser" });

        await Assert.ThrowsAsync<Exception>(() =>
            _authService.RegisterAsync("testuser", "password123"));
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnUser_WhenPasswordIsCorrect()
    {
        var password = "password123";
        var hashed = typeof(AuthService).GetMethod("HashPassword", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                                        .Invoke(null, new object[] { password }) as string;

        var existingUser = new User { Id = Guid.NewGuid(), Username = "testuser", PasswordHash = hashed };
        _userRepoMock.Setup(r => r.GetByUsernameAsync("testuser"))
                     .ReturnsAsync(existingUser);

        var user = await _authService.LoginAsync("testuser", password);

        Assert.NotNull(user);
        Assert.Equal("testuser", user.Username);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenUserNotFound()
    {
        _userRepoMock.Setup(r => r.GetByUsernameAsync("nouser"))
                     .ReturnsAsync((User)null);

        var user = await _authService.LoginAsync("nouser", "any");
        Assert.Null(user);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenPasswordIsIncorrect()
    {
        var password = "password123";
        var hashed = typeof(AuthService).GetMethod("HashPassword", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                                        .Invoke(null, new object[] { password }) as string;

        var existingUser = new User { Id = Guid.NewGuid(), Username = "testuser", PasswordHash = hashed };
        _userRepoMock.Setup(r => r.GetByUsernameAsync("testuser"))
                     .ReturnsAsync(existingUser);

        var user = await _authService.LoginAsync("testuser", "wrongpassword");
        Assert.Null(user);
    }
}
