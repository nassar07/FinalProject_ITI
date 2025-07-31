using FinalProject_ITI.DTO;
using FinalProject_ITI.Models;
using FinalProject_ITI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Server;

namespace FinalProject_ITI.Controllers;

//[Authorize(Roles = "Admin")]
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

    [HttpPut("UpdateBazaar")]
    public async Task<IActionResult> UpdateBazaar(BazarDTO Bazar)
    {
        var existingBazaar = await _bazarRepository.GetById(Bazar.Id);
        if (existingBazaar == null)
            return NotFound();

        existingBazaar.Title = Bazar.Title;
        existingBazaar.EventDate = Bazar.EventDate;
        existingBazaar.StartTime = Bazar.StartTime;
        existingBazaar.EndTime = Bazar.EndTime;
        existingBazaar.Location = Bazar.Location;
        existingBazaar.Entry = Bazar.Entry;

        _bazarRepository.Update(existingBazaar);
        await _bazarRepository.SaveChanges();

        return Ok(new { message = "Updated" });
    }

    [HttpPost("CreateBazaar")]
    public async Task<IActionResult> CreateBazaar(Bazar Bazar)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
      
        await _bazarRepository.Add(Bazar);
        await _bazarRepository.SaveChanges();
        return Ok(new { message = "Add new Bazar Successfully" });
    }

    [HttpDelete("Delete/{id:int}")]
    public async Task<IActionResult> DeleteBazaar(int id)
    {
        var bazaar = await _bazarRepository.GetById(id);
        if (bazaar == null)
            return NotFound();
        _bazarRepository.Delete(bazaar);
        await _bazarRepository.SaveChanges();
        return Ok(new { message = "deleted" });
    }

    [AllowAnonymous]
    [HttpGet("next-event")]
    public async Task<IActionResult> GetBazaarEventById()
    {
        var bazaar = await _bazarRepository.GetQuery().OrderByDescending(b=>b.EventDate).FirstOrDefaultAsync();
           
        if (bazaar == null)
            return NotFound(new { message = "No Bazzar already Available" });
        return Ok(bazaar);
    }

}














