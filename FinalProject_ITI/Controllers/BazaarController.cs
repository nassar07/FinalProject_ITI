using FinalProject_ITI.DTO;
using FinalProject_ITI.Models;
using FinalProject_ITI.Repositories.Interfaces;
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

        private readonly IRepository<Bazar> _bazarRepository;

        public BazaarController(IRepository<Bazar> bazarRepository)
        {
            _bazarRepository = bazarRepository;
        }




        [HttpGet("GetAllBazaars")]
        public async Task<IActionResult> GetAllBazaars()
        {
            var bazaars = await _bazarRepository.GetAll();
            return Ok(bazaars);
        }


        [HttpGet("GetBazaarById/{id:int}")]
        public async Task<IActionResult> GetBazaarById(int id)
        {
            var bazaar = await _bazarRepository.GetById(id);


            if (bazaar == null)
                return NotFound();

            return Ok(bazaar);
        }



        [HttpPut("UpdateBazaar/{id:int}")]
        public async Task<IActionResult> UpdateBazaar(int id, [FromBody] CreateBazarDTO dto)
        {
            var existingBazaar = await _bazarRepository.GetById(id);
            if (existingBazaar == null)
                return NotFound();

            existingBazaar.Title = dto.Title;
            existingBazaar.EventDate = dto.EventDate;
            existingBazaar.StartTime = dto.StartTime;
            existingBazaar.EndTime = dto.EndTime;
            existingBazaar.Location = dto.Location;
            existingBazaar.Entry = dto.Entry;

            _bazarRepository.Update(existingBazaar);
            await _bazarRepository.SaveChanges();

            return NoContent();
        }




        [HttpPost("CreateBazaar")]
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

            await _bazarRepository.Add(bazar);
            await _bazarRepository.SaveChanges();

            return CreatedAtAction(nameof(GetBazaarById), new { id = bazar.Id }, bazar);
        }



        [HttpDelete("Delete/{id:int}")]
        public async Task<IActionResult> DeleteBazaar(int id)
        {
            var bazaar = await _bazarRepository.GetById(id);
            if (bazaar == null)
                return NotFound();

            _bazarRepository.Delete(bazaar);
            await _bazarRepository.SaveChanges();

            return NoContent();
        }



        [AllowAnonymous]
        [HttpGet("next-event/{id}")]
        public async Task<IActionResult> GetBazaarEventById(int id)
        {
            var bazaar = await _bazarRepository.GetQuery()
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














