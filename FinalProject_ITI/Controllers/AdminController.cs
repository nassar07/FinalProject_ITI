using FinalProject_ITI.DTO;
using FinalProject_ITI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalProject_ITI.Controllers;

//[Authorize(Roles = "ADMIN")]
[Route("api/[controller]")]
[ApiController]
public class AdminController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    public AdminController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }
    [HttpPost("promotion/{userId}")]
    public async Task<IActionResult> PromoteToAdmin(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest(new { message = "User ID cannot be null or empty." });

        var user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            await _userManager.AddToRoleAsync(user, "ADMIN");
            return Ok(new { message = "User promoted to admin successfully." });
        }
        return BadRequest(new { message = "failed to add user" });
    }

    [HttpPost("demotion/{userId}")]
    public async Task<IActionResult> DemoteToUser(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest(new { message = "User ID cannot be null or empty." });

        var user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            var result = await _userManager.RemoveFromRoleAsync(user, "ADMIN");
            if (result.Succeeded)
                return Ok(new { message = "User demoted from admin successfully." });

            return BadRequest(new { message = "Failed to remove user from ADMIN role." });
        }

        return BadRequest(new { message = "User not found." });
    }

    [HttpGet("AllUsers")]
    public async Task<IActionResult> UsersList()
    {
        var allUsers = await _userManager.Users.ToListAsync();

        var userDtos = new List<UserDTO>();

        foreach (var user in allUsers)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var dto = new UserDTO
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Roles = roles.ToList()
            };

            userDtos.Add(dto);
        }

        return Ok(userDtos);
    }

    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest(new { message = "User ID cannot be null or empty." });

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { message = "User not found." });

        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
            return Ok(new { message = "User deleted successfully." });

        return BadRequest(new
        {
            message = "Failed to delete user.",
            errors = result.Errors.Select(e => e.Description)
        });
    }

}
