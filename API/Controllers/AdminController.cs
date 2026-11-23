
using Microsoft.AspNetCore.Mvc;
using Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace API.Controllers;


[ApiController]
[Route("api/[controller]")]
// [Authorize(Roles = "Admin")]
public class AdminController(RoleManager<IdentityRole> roleManager): ControllerBase
{
	private readonly RoleManager<IdentityRole> _roleManager = roleManager;

	[HttpGet("roles")]
	public ActionResult<IEnumerable<string>> GetRoles()
	{
		var roles = _roleManager.Roles
			.Select(r => r.Name)
			.Where(n => !string.IsNullOrWhiteSpace(n))
			.OrderBy(n => n)
			.Cast<string>()
			.ToList();

		return Ok(roles);
	}

	[HttpPost("roles")]
	public async Task<IActionResult> AddRole([FromBody] CreateRoleRequest request)
	{
		if (string.IsNullOrWhiteSpace(request.RoleName))
		{
			return BadRequest(new { error = "Role name is required" });
		}

		var roleName = request.RoleName.Trim();

		if (await _roleManager.RoleExistsAsync(roleName))
		{
			return Conflict(new { error = $"Role '{roleName}' already exists" });
		}

		var result = await _roleManager.CreateAsync(new IdentityRole(roleName));

		if (!result.Succeeded)
		{
			return BadRequest(new
			{
				error = "Role creation failed",
				details = result.Errors.Select(e => e.Description)
			});
		}

		return Ok(new { message = $"Role '{roleName}' created" });
	}


}