using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FinalProject_ITI.DTO;
using FinalProject_ITI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace FinalProject_ITI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    public UserManager<ApplicationUser> userManager { get; }

    public AccountController(UserManager<ApplicationUser> userManager)
    {
        this.userManager = userManager;
    }

    [HttpPost("Register")]
    public async Task<IActionResult> Register(RegisterDTO userFromRequest)
    {
        if (ModelState.IsValid)
        {
            ApplicationUser user = new ApplicationUser();
            user.FirstName = userFromRequest.FirstName;
            user.LastName = userFromRequest.LastName;
            user.Email = userFromRequest.Email;
            user.UserName = userFromRequest.Email;
            user.PhoneNumber = userFromRequest.PhoneNumber;
            user.AccountType = userFromRequest.AccountType;


            IdentityResult result = await userManager.CreateAsync(user, userFromRequest.Password);

            if (result.Succeeded)
            {
                if (userFromRequest.AccountType == "Customer" || userFromRequest.AccountType == "BrandOwner")
                {
                    await userManager.AddToRoleAsync(user, userFromRequest.AccountType);
                }
                return Ok("Created");
            }
            foreach (var item in result.Errors)
            {
                ModelState.AddModelError("Password", item.Description);
            }
        }
        return BadRequest(ModelState);
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login(LoginDTO userFromRequest)
    {
        if (ModelState.IsValid)
        {
            ApplicationUser userFromDb = await userManager.FindByEmailAsync(userFromRequest.Email);

            if (userFromDb != null)
            {
               bool Found = await userManager.CheckPasswordAsync(userFromDb, userFromRequest.Password);

               if (Found == true)
                {

                    List<Claim> UserClaim = new List<Claim>();

                    UserClaim.Add(new Claim(ClaimTypes.NameIdentifier, userFromDb.Id));
                    UserClaim.Add(new Claim(ClaimTypes.Email, userFromDb.Email));
                    UserClaim.Add(new Claim(JwtRegisteredClaimNames.Jti , Guid.NewGuid().ToString()));

                    var UserRoles = await userManager.GetRolesAsync(userFromDb);
                    foreach (var roleName in UserRoles)
                    { 
                        UserClaim.Add(new Claim(ClaimTypes.Role, roleName));
                        
                    }

                    SymmetricSecurityKey SignKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("hfsbvdfjknsfkns@44&&%%$$dcskln1548vkls2sdbfbdnklf554d$$##"));

                    SigningCredentials signingCred = new SigningCredentials(SignKey, SecurityAlgorithms.HmacSha256);

                    JwtSecurityToken myToken = new JwtSecurityToken(

                        issuer: "http://localhost:5066/",
                        audience:"http://localhost:5127",
                        expires: DateTime.Now.AddHours(1),
                        claims: UserClaim,
                        signingCredentials: signingCred
                    );

                    return Ok(
                    new MyToken
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(myToken),
                        expiration = DateTime.Now.AddHours(1)
                    });
               }
            }
            ModelState.AddModelError("Password", "Invalid Email OR Password");
        }
        return BadRequest(ModelState);
    }
}
