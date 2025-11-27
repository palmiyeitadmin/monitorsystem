using ERAMonitor.Core.DTOs;
using ERAMonitor.Core.Entities;
using ERAMonitor.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ERAMonitor.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public LocationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LocationDto>>> GetLocations()
    {
        var locations = await _context.Locations
            .Select(l => new LocationDto
            {
                Id = l.Id,
                Name = l.Name,
                Category = l.Category.ToString(),
                ProviderName = l.ProviderName,
                City = l.City,
                Country = l.Country,
                IsActive = l.IsActive
            })
            .ToListAsync();

        return Ok(locations);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<LocationDto>> GetLocation(Guid id)
    {
        var location = await _context.Locations.FindAsync(id);

        if (location == null)
        {
            return NotFound();
        }

        return new LocationDto
        {
            Id = location.Id,
            Name = location.Name,
            Category = location.Category.ToString(),
            ProviderName = location.ProviderName,
            City = location.City,
            Country = location.Country,
            IsActive = location.IsActive
        };
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<LocationDto>> CreateLocation(CreateLocationRequest request)
    {
        var orgId = Guid.Parse(User.FindFirstValue("OrganizationId")!);

        var location = new Location
        {
            OrganizationId = orgId,
            Name = request.Name,
            Category = request.Category,
            ProviderName = request.ProviderName,
            City = request.City,
            Country = request.Country,
            IsActive = true
        };

        _context.Locations.Add(location);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetLocation), new { id = location.Id }, new LocationDto
        {
            Id = location.Id,
            Name = location.Name,
            Category = location.Category.ToString(),
            ProviderName = location.ProviderName,
            City = location.City,
            Country = location.Country,
            IsActive = location.IsActive
        });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> UpdateLocation(Guid id, UpdateLocationRequest request)
    {
        var location = await _context.Locations.FindAsync(id);

        if (location == null)
        {
            return NotFound();
        }

        location.Name = request.Name;
        location.Category = request.Category;
        location.ProviderName = request.ProviderName;
        location.City = request.City;
        location.Country = request.Country;
        location.IsActive = request.IsActive;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!LocationExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> DeleteLocation(Guid id)
    {
        var location = await _context.Locations.FindAsync(id);
        if (location == null)
        {
            return NotFound();
        }

        _context.Locations.Remove(location);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool LocationExists(Guid id)
    {
        return _context.Locations.Any(e => e.Id == id);
    }
}
