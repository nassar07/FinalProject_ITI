using FinalProject_ITI.DTO;
using FinalProject_ITI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Server;

namespace FinalProject_ITI.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class BazaarController : ControllerBase
    {

        private readonly ITIContext _context;

        public BazaarController(ITIContext context)
        {
            _context = context;
        }



        [HttpGet]
        public async Task<IActionResult> GetAllBazaars()
        {
            var bazaars = await _context.Bazars.ToListAsync();
            return Ok(bazaars);
        }


        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetBazaarById(int id)
        {
            var bazaar = await _context.Bazars
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bazaar == null)
                return NotFound();

            return Ok(bazaar);
        }



        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateBazaar(int id, [FromBody] CreateBazarDTO dto)
        {
            var existingBazaar = await _context.Bazars
                .FirstOrDefaultAsync(b => b.Id == id);

            if (existingBazaar == null)
                return NotFound();

            existingBazaar.Title = dto.Title;
            existingBazaar.EventDate = dto.EventDate;
            existingBazaar.StartTime = dto.StartTime;
            existingBazaar.EndTime = dto.EndTime;
            existingBazaar.Location = dto.Location;
            existingBazaar.Entry = dto.Entry;

            await _context.SaveChangesAsync();
            return NoContent();
        }




        [HttpPost]
        public async Task<IActionResult> CreateBazaar([FromBody] CreateBazarDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var bazar = new Bazar
            {
                Title = dto.Title,
                EventDate = dto.EventDate,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Location = dto.Location,
                Entry = dto.Entry,

            };

            _context.Bazars.Add(bazar);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBazaarById), new { id = bazar.Id }, bazar);
        }


        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteBazaar(int id)
        {
            var bazaar = await _context.Bazars
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bazaar == null)
                return NotFound();

            _context.Bazars.Remove(bazaar);
            await _context.SaveChangesAsync();

            return NoContent();
        }



        [AllowAnonymous]
        [HttpGet("next-event/{id}")]
        public async Task<IActionResult> GetBazaarEventById(int id)
        {
            var bazaar = await _context.Bazars
                .Where(b => b.Id == id)
                .Select(b => new
                {
                    b.Id,
                    Title = b.Title,
                    Date = b.EventDate.ToString("MMMM dd, yyyy"),
                    DateTime = $"{b.EventDate:dddd, MMMM dd, yyyy} • {b.StartTime} - {b.EndTime}",
                    Location = b.Location,
                    Entry = b.Entry
                })
                .FirstOrDefaultAsync();

            if (bazaar == null)
                return NotFound();

            return Ok(bazaar);
        }


    }
}






    







