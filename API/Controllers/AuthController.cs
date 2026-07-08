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
        var user = new AppUser
        {
            UserName = request.Email,
            Email = request.Email
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var validationErrors = result.Errors
                .GroupBy(error => error.Code)
                .ToDictionary(group => group.Key, group => group.Select(error => error.Description).ToArray());

            return ValidationProblem(new ValidationProblemDetails(validationErrors));
        }

        return Ok();
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthTokenResponse>> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null)
        {
            return Problem(title: "Failed", statusCode: StatusCodes.Status401Unauthorized);
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            return Problem(title: result.IsLockedOut ? "LockedOut" : result.RequiresTwoFactor ? "RequiresTwoFactor" : "Failed", statusCode: StatusCodes.Status401Unauthorized);
        }

        var response = await IssueTokensAsync(user);
        SetRefreshTokenCookie(response.RefreshToken);

        return Ok(new AuthTokenResponse
        {
            TokenType = response.TokenType,
            AccessToken = response.AccessToken,
            ExpiresIn = response.ExpiresIn,
            Email = user.Email
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
            return Unauthorized();
        }

        var refreshTokenProtector = _bearerTokenOptions.Get(IdentityConstants.BearerScheme).RefreshTokenProtector;
        var refreshTicket = refreshTokenProtector.Unprotect(refreshToken);

        if (refreshTicket?.Properties?.ExpiresUtc is not { } expiresUtc || _timeProvider.GetUtcNow() >= expiresUtc)
        {
            DeleteRefreshTokenCookie();
            return Unauthorized();
        }

        var user = await _signInManager.ValidateSecurityStampAsync(refreshTicket.Principal) as AppUser;

        if (user is null)
        {
            DeleteRefreshTokenCookie();
            return Unauthorized();
        }

        var response = await IssueTokensAsync(user);
        SetRefreshTokenCookie(response.RefreshToken);

        return Ok(new AuthTokenResponse
        {
            TokenType = response.TokenType,
            AccessToken = response.AccessToken,
            ExpiresIn = response.ExpiresIn,
            Email = user.Email
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        DeleteRefreshTokenCookie();

        return Ok(new { Message = "Logged out successfully" });
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