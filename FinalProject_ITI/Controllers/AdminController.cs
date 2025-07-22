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
            return BadRequest("User ID cannot be null or empty.");

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

}
