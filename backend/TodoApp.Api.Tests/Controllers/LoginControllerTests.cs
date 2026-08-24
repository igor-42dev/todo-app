using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using TodoApp.Api.Controllers;
using TodoApp.Api.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace TodoApp.Api.Tests.Controllers;

public class LoginControllerTests
{
    private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
    private readonly Mock<SignInManager<IdentityUser>> _signInManagerMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly LoginController _controller;

    public LoginControllerTests()
    {
        var userStoreMock = new Mock<IUserStore<IdentityUser>>();
        _userManagerMock = new Mock<UserManager<IdentityUser>>(
            userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        var contextAccessorMock = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var userClaimsPrincipalMock = new Mock<System.Security.Claims.ClaimsPrincipal>();
        var userPrincipalFactoryMock = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();

        _signInManagerMock = new Mock<SignInManager<IdentityUser>>(
            _userManagerMock.Object,
            contextAccessorMock.Object,
            userPrincipalFactoryMock.Object,
            null!,
            null!,
            null!,
            null!);

        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.Setup(c => c["Jwt:SecretKey"]).Returns("6ba265a1ca9a4c988564e19e898d486c95427294b05f41f0bb0a4e228ed7b4ef");
        _configurationMock.Setup(c => c["Jwt:Issuer"]).Returns("TodoApp.Api");
        _configurationMock.Setup(c => c["Jwt:Audience"]).Returns("TodoApp.Frontend");
        _configurationMock.Setup(c => c["Jwt:ExpirationInMinutes"]).Returns("60");

        _controller = new LoginController(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _configurationMock.Object);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkComToken()
    {
        var user = new IdentityUser { Id = "test-id", UserName = "test@email.com", Email = "test@email.com" };
        _userManagerMock.Setup(m => m.FindByEmailAsync("test@email.com")).ReturnsAsync(user);
        _signInManagerMock.Setup(m => m.CheckPasswordSignInAsync(user, "Password123", false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        var request = new LoginRequest { Email = "test@email.com", Senha = "Password123" };

        var result = await _controller.Login(request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<Models.Responses.LoginResponse>().Subject;
        response.Token.Should().NotBeEmpty();
        response.Expiration.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_InvalidEmail_ReturnsUnauthorized()
    {
        _userManagerMock.Setup(m => m.FindByEmailAsync("invalid@email.com"))
            .ReturnsAsync((IdentityUser?)null);

        var request = new LoginRequest { Email = "invalid@email.com", Senha = "Password123" };

        var result = await _controller.Login(request);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        var user = new IdentityUser { Id = "test-id", UserName = "test@email.com", Email = "test@email.com" };
        _userManagerMock.Setup(m => m.FindByEmailAsync("test@email.com")).ReturnsAsync(user);
        _signInManagerMock.Setup(m => m.CheckPasswordSignInAsync(user, "WrongPassword", false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

        var request = new LoginRequest { Email = "test@email.com", Senha = "WrongPassword" };

        var result = await _controller.Login(request);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }
}
