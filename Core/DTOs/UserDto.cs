namespace Core.DTOs;

public sealed class UserDto
{
	public string Id { get; set; } = string.Empty;
	public string? Email { get; set; }
	public string? UserName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
	public List<string> Roles { get; set; } = new();
	public List<string> AssignedRoles { get; set; } = new();
	public List<string> RemainingRoles { get; set; } = new();
}


public sealed class AddUserRoleRequest
{
	public string UserId { get; set; } = string.Empty;
	public string Role { get; set; } = string.Empty;
}