using FinalProject_ITI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
           

            var bazars = await _context.Bazars.ToListAsync();
            return Ok(bazars);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetBazaarById(int id)
        {
            var bazar = await _context.Bazars.FindAsync(id);
            if (bazar == null)
                return NotFound();

            return Ok(bazar);


           
        }

        [HttpPost]
        public async Task<IActionResult> CreateBazaar([FromBody] Bazar bazar)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Bazars.Add(bazar);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBazaarById), new { id = bazar.Id }, bazar);

        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateBazaar(int id, [FromBody] Bazar bazar)
        {
            if (id != bazar.Id)
                return BadRequest("not found");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Entry(bazar).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Bazars.Any(b => b.Id == id))
                    return NotFound();

                throw;
            }

            return NoContent();
        }

      
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteBazaar(int id)
        {
            var bazaar = await _context.Bazars.FindAsync(id);
            if (bazaar == null)
                return NotFound();

            _context.Bazars.Remove(bazaar);
            await _context.SaveChangesAsync();

            return NoContent();
        }

     
        [AllowAnonymous]
        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestBazaarEvent()
        {
            var nextBazaar = await _context.Bazars
                  .OrderByDescending(b => b.EventDate)
                .Select(b => new
                {
                    b.Id,
                    Title = b.Title,
                    Date = b.EventDate.ToString("MMMM dd, yyyy"),
                    DateTime = $"{b.EventDate:dddd, MMMM dd, yyyy} • {b.StartTime} - {b.EndTime}",
                    Location = b.Location,
                    Entry = b.Entry,
                    Highlights = b.Highlights.Select(h => new { h.Icon, h.Text })
                })
                .FirstOrDefaultAsync();

            if (nextBazaar== null)
                return NotFound();

            return Ok(nextBazaar);
        }

    }
}
