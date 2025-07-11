using FinalProject_ITI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinalProject_ITI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivitesController : ControllerBase
    {

        private readonly ITIContext _context;

        public ActivitesController(ITIContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetActivities()
        {
            var activities = await _context.BazarActivities.ToListAsync();
            return Ok(activities);
        }

    }
}
