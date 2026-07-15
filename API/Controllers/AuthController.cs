using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using API.DTOs;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace API.Controllers;

[ApiController]
[Route("api")]
public class AuthController(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    IOptionsMonitor<BearerTokenOptions> bearerTokenOptions,
    TimeProvider timeProvider) : ControllerBase
{
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly SignInManager<AppUser> _signInManager = signInManager;
    private readonly IOptionsMonitor<BearerTokenOptions> _bearerTokenOptions = bearerTokenOptions;
    private readonly TimeProvider _timeProvider = timeProvider;

    private const string RefreshTokenCookieName = "skyshop.refreshToken";
    private static readonly JsonSerializerOptions WebJsonOptions = new(JsonSerializerDefaults.Web);

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {

        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            return BadRequest(new { error = "User with this email already exists" });
        }

        var user = new AppUser
        {
            UserName = request.Email,
            Email = request.Email
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if(!result.Succeeded)
        {
            return BadRequest("Error in creating the user");
        }

        var roleResult = await _userManager.AddToRoleAsync(user, "User");

        if (!roleResult.Succeeded)
        {
            return BadRequest("Error in assigning role to the user");
        }

        return Ok(new { message ="Successfully registered"});
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthTokenResponse>> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null)
        {
            return NotFound("User not found");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            return BadRequest("specified password is incorrect");
        }

        var roles = await _userManager.GetRolesAsync(user);

        var response = await IssueTokensAsync(user);
        SetRefreshTokenCookie(response.RefreshToken);

        return Ok(new AuthTokenResponse
        {
            TokenType = response.TokenType,
            AccessToken = response.AccessToken,
            ExpiresIn = response.ExpiresIn,
            Email = user.Email,
            Roles = roles.ToList()
        });
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthTokenResponse>> Refresh()
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName];

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            DeleteRefreshTokenCookie();
            return NoContent();
        }

        var refreshTokenProtector = _bearerTokenOptions.Get(IdentityConstants.BearerScheme).RefreshTokenProtector;
        var refreshTicket = refreshTokenProtector.Unprotect(refreshToken);

        if (refreshTicket?.Properties?.ExpiresUtc is not { } expiresUtc || _timeProvider.GetUtcNow() >= expiresUtc)
        {
            DeleteRefreshTokenCookie();
            return NoContent();
        }

        var user = await _signInManager.ValidateSecurityStampAsync(refreshTicket.Principal) as AppUser;

        if (user is null)
        {
            DeleteRefreshTokenCookie();
            return StatusCode(StatusCodes.Status401Unauthorized, "Invalid refresh token");
        }

        var roles = await _userManager.GetRolesAsync(user);

        var response = await IssueTokensAsync(user);
        SetRefreshTokenCookie(response.RefreshToken);

        return Ok(new AuthTokenResponse
        {
            TokenType = response.TokenType,
            AccessToken = response.AccessToken,
            ExpiresIn = response.ExpiresIn,
            Email = user.Email,
            Roles = roles.ToList()
        });
    }


    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        DeleteRefreshTokenCookie();

        return Ok("Logged out successfully");
    }

    private async Task<FrameworkBearerTokenResponse> IssueTokensAsync(AppUser user)
    {
        var principal = await _signInManager.CreateUserPrincipalAsync(user);
        var originalBody = Response.Body;
        var originalContentType = Response.ContentType;
        var originalStatusCode = Response.StatusCode;

        await using var capture = new MemoryStream();
        Response.Body = capture;

        try
        {
            await HttpContext.SignInAsync(IdentityConstants.BearerScheme, principal);
            capture.Position = 0;

            var frameworkResponse = await JsonSerializer.DeserializeAsync<FrameworkBearerTokenResponse>(capture, WebJsonOptions)
                ?? throw new InvalidOperationException("Failed to generate bearer token response.");

            return frameworkResponse;
        }
        finally
        {
            Response.Body = originalBody;
            Response.ContentType = originalContentType;
            Response.StatusCode = originalStatusCode;
            Response.Headers.Remove("Content-Length");
        }
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        var refreshExpiration = _bearerTokenOptions.Get(IdentityConstants.BearerScheme).RefreshTokenExpiration;

        Response.Cookies.Append(RefreshTokenCookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Path = "/api",
            Expires = _timeProvider.GetUtcNow().Add(refreshExpiration)
        });
    }

    private void DeleteRefreshTokenCookie()
    {
        Response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions
        {
            Path = "/api"
        });
    }

    private sealed record FrameworkBearerTokenResponse(string TokenType, string AccessToken, long ExpiresIn, string RefreshToken);
}