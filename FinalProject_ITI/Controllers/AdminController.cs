using FinalProject_ITI.Models;
using FinalProject_ITI.Repositories.Implementations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalProject_ITI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AdminController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly Repository<Bazar> _Bazar;
    public AdminController(UserManager<ApplicationUser> userManager, Repository<Bazar> Bazar)
    {
        _userManager = userManager;
        _Bazar = Bazar;
    }
    [HttpPost("promotion")]
    public async Task<IActionResult> PromoteToAdmin(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            BadRequest("User ID cannot be null or empty.");

        var user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            await _userManager.AddToRoleAsync(user, "ADMIN");
            return Ok("User promoted to admin successfully.");
        }
        return BadRequest("failed to add user");
    }
    [HttpGet("AllUsers")]
    public async Task<IActionResult> UsersList()
    {
        var allUsers = await _userManager.Users.ToListAsync();

        var normalUsers = new List<ApplicationUser>();

        foreach (var user in allUsers)
        {
            if (!await _userManager.IsInRoleAsync(user, "ADMIN"))
            {
                normalUsers.Add(user);

            }
        }
        return Ok(normalUsers);
    }

}
