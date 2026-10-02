
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Core.DTOs;

namespace API.Controllers;


[ApiController]
[Route("api/[controller]")]
public class UserController(BookContext dbContext): ControllerBase
{
	private readonly BookContext _dbContext = dbContext;

	// [Authorize(Roles = "Admin")]
	[HttpGet]
	public async Task<ActionResult<IReadOnlyList<UserDto>>> GetAllUsersWithRoles()
	{
		var allRoles = await _dbContext.Roles
			.AsNoTracking()
			.Select(r => r.Name)
			.Where(name => !string.IsNullOrWhiteSpace(name))
			.Select(name => name!)
			.Distinct()
			.OrderBy(name => name)
			.ToListAsync();

		var rows = await (
			from user in _dbContext.Users.AsNoTracking()
			join userRole in _dbContext.UserRoles.AsNoTracking()
				on user.Id equals userRole.UserId into userRoleGroup
			from userRole in userRoleGroup.DefaultIfEmpty()
			join role in _dbContext.Roles.AsNoTracking()
				on userRole.RoleId equals role.Id into roleGroup
			from role in roleGroup.DefaultIfEmpty()
			select new
			{
				user.Id,
				user.Email,
				user.UserName,
				RoleName = role != null ? role.Name : null
			})
			.ToListAsync();

		var responses = rows
			.GroupBy(x => new { x.Id, x.Email, x.UserName })
			.Select(g =>
			{
				var assignedRoles = g
					.Where(x => !string.IsNullOrWhiteSpace(x.RoleName))
					.Select(x => x.RoleName!)
					.Distinct(StringComparer.OrdinalIgnoreCase)
					.OrderBy(name => name)
					.ToList();

				var assignedLookup = assignedRoles
					.ToHashSet(StringComparer.OrdinalIgnoreCase);

				var remainingRoles = allRoles
					.Where(roleName => !assignedLookup.Contains(roleName))
					.ToList();

				return new UserDto
				{
					Id = g.Key.Id,
					Email = g.Key.Email,
					UserName = g.Key.UserName,
					Roles = assignedRoles,
					AssignedRoles = assignedRoles,
					RemainingRoles = remainingRoles
				};
			})
			.ToList();

		return Ok(responses);
	}

	[HttpDelete]
	public async Task<IActionResult> RemoveUserRole([FromQuery] string userId, [FromQuery] string role)
	{
		if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(role))
		{
			return BadRequest("userId and role are required.");
		}

		var normalizedUserId = userId.Trim();
		var roleInput = role.Trim();

		var roleEntity = await _dbContext.Roles
			.AsNoTracking()
			.Where(r => r.Id == roleInput || r.Name == roleInput)
			.Select(r => new { r.Id })
			.FirstOrDefaultAsync();

		if (roleEntity is null)
		{
			return NotFound("Role not found.");
		}

		var userRole = await _dbContext.UserRoles
			.FirstOrDefaultAsync(ur => ur.UserId == normalizedUserId && ur.RoleId == roleEntity.Id);

		if (userRole is null)
		{
			return NotFound();
		}

		_dbContext.UserRoles.Remove(userRole);
		await _dbContext.SaveChangesAsync();

		return NoContent();
	}

	[HttpGet("getRoles")]
	public async Task<ActionResult<IEnumerable<string>>> GetRoles()
	{
		var roles = await _dbContext.Roles
			.AsNoTracking()
			.Select(r => r.Name)
			.Where(n => !string.IsNullOrWhiteSpace(n))
			.OrderBy(n => n)
			.ToListAsync();

		return Ok(roles);
	}

	[HttpPost("addRole")]
	public async Task<IActionResult> AddUserRole([FromBody] AddUserRoleRequest request)
	{
		if (request is null || string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.Role))
		{
			return BadRequest("UserId and Role are required.");
		}

		var userId = request.UserId.Trim();
		var roleInput = request.Role.Trim();

		var userExists = await _dbContext.Users
			.AsNoTracking()
			.AnyAsync(u => u.Id == userId);

		if (!userExists)
		{
			return NotFound("User not found.");
		}

		var role = await _dbContext.Roles
			.AsNoTracking()
			.Where(r => r.Id == roleInput || r.Name == roleInput)
			.Select(r => new { r.Id, r.Name })
			.FirstOrDefaultAsync();

		if (role is null)
		{
			return NotFound("Role not found.");
		}

		var alreadyAssigned = await _dbContext.UserRoles
			.AsNoTracking()
			.AnyAsync(ur => ur.UserId == userId && ur.RoleId == role.Id);

		if (alreadyAssigned)
		{
			return Conflict("This role is already assigned to the user.");
		}

		var userRole = new IdentityUserRole<string>
		{
			UserId = userId,
			RoleId = role.Id
		};

		_dbContext.UserRoles.Add(userRole);
		await _dbContext.SaveChangesAsync();

		return Ok();
	}

}