namespace API.Tests.Controllers;

using API.Controllers;
using Core.DTOs;
using FluentAssertions;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

public class AuthControllerTests
{
	[Fact]
	public async Task Register_WhenUserAlreadyExists_ReturnsBadRequest()
	{
		// Arrange
		var userManagerMock = CreateUserManagerMock();
		var signInManagerMock = CreateSignInManagerMock(userManagerMock.Object);
		var bearerOptionsMock = new Mock<IOptionsMonitor<BearerTokenOptions>>();
		var controller = new AuthController(
			userManagerMock.Object,
			signInManagerMock.Object,
			bearerOptionsMock.Object,
			TimeProvider.System);

		var request = new RegisterRequest
		{
			Email = "existing@skyshop.test",
			Password = "P@ssw0rd!"
		};

		userManagerMock
			.Setup(x => x.FindByEmailAsync(request.Email))
			.ReturnsAsync(new AppUser { Email = request.Email, UserName = request.Email });

		// Act
		var result = await controller.Register(request);

		// Assert
		result.Should().BeOfType<OkObjectResult>();
		userManagerMock.Verify(x => x.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()), Times.Never);
		userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<AppUser>(), It.IsAny<string>()), Times.Never);
	}


	[Fact]
	public async Task Register_WhenRequestIsValid_ReturnsOk()
	{
		// Arrange
		var userManagerMock = CreateUserManagerMock();
		var signInManagerMock = CreateSignInManagerMock(userManagerMock.Object);
		var bearerOptionsMock = new Mock<IOptionsMonitor<BearerTokenOptions>>();
		var controller = new AuthController(
			userManagerMock.Object,
			signInManagerMock.Object,
			bearerOptionsMock.Object,
			TimeProvider.System);

		var request = new RegisterRequest
		{
			Email = "new@skyshop.test",
			Password = "P@ssw0rd!"
		};

		userManagerMock
			.Setup(x => x.FindByEmailAsync(request.Email))
			.ReturnsAsync((AppUser?)null);

		userManagerMock
			.Setup(x => x.CreateAsync(It.IsAny<AppUser>(), request.Password))
			.ReturnsAsync(IdentityResult.Success);

		userManagerMock
			.Setup(x => x.AddToRoleAsync(It.IsAny<AppUser>(), "User"))
			.ReturnsAsync(IdentityResult.Success);

		// Act
		var result = await controller.Register(request);

		// Assert
		result.Should().BeOfType<OkObjectResult>();
		userManagerMock.Verify(x => x.CreateAsync(
				It.Is<AppUser>(u => u.Email == request.Email && u.UserName == request.Email),
				request.Password),
			Times.Once);
		userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<AppUser>(), "User"), Times.Once);
	}

	private static Mock<UserManager<AppUser>> CreateUserManagerMock()
	{
		var userStore = new Mock<IUserStore<AppUser>>();

		return new Mock<UserManager<AppUser>>(
			userStore.Object,
			null!,
			null!,
			null!,
			null!,
			null!,
			null!,
			null!,
			null!);
	}

	private static Mock<SignInManager<AppUser>> CreateSignInManagerMock(UserManager<AppUser> userManager)
	{
		return new Mock<SignInManager<AppUser>>(
			userManager,
			new Mock<IHttpContextAccessor>().Object,
			new Mock<IUserClaimsPrincipalFactory<AppUser>>().Object,
			Options.Create(new IdentityOptions()),
			new Mock<ILogger<SignInManager<AppUser>>>().Object,
			new Mock<IAuthenticationSchemeProvider>().Object,
			new Mock<IUserConfirmation<AppUser>>().Object);
	}
}
