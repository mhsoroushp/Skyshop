namespace API.DTOs;

public sealed class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed class AuthTokenResponse
{
    public string TokenType { get; set; } = "Bearer";
    public string AccessToken { get; set; } = string.Empty;
    public long ExpiresIn { get; set; }
    public string? Email { get; set; }
    public List<string> Roles { get; set; } = new();
}