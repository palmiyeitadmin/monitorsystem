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
public class CustomersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CustomersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomers()
    {
        var customers = await _context.Customers
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                LogoUrl = c.LogoUrl,
                Industry = c.Industry,
                IsActive = c.IsActive,
                ContactName = c.ContactName,
                ContactEmail = c.ContactEmail
            })
            .ToListAsync();

        return Ok(customers);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetCustomer(Guid id)
    {
        var customer = await _context.Customers.FindAsync(id);

        if (customer == null)
        {
            return NotFound();
        }

        return new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Slug = customer.Slug,
            LogoUrl = customer.LogoUrl,
            Industry = customer.Industry,
            IsActive = customer.IsActive,
            ContactName = customer.ContactName,
            ContactEmail = customer.ContactEmail
        };
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<CustomerDto>> CreateCustomer(CreateCustomerRequest request)
    {
        var orgId = Guid.Parse(User.FindFirstValue("OrganizationId")!);

        if (await _context.Customers.AnyAsync(c => c.OrganizationId == orgId && c.Slug == request.Slug))
        {
            return BadRequest("Customer with this slug already exists in your organization");
        }

        var customer = new Customer
        {
            OrganizationId = orgId,
            Name = request.Name,
            Slug = request.Slug,
            Industry = request.Industry,
            ContactName = request.ContactName,
            ContactEmail = request.ContactEmail,
            IsActive = true
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Slug = customer.Slug,
            Industry = customer.Industry,
            IsActive = customer.IsActive,
            ContactName = customer.ContactName,
            ContactEmail = customer.ContactEmail
        });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> UpdateCustomer(Guid id, UpdateCustomerRequest request)
    {
        var customer = await _context.Customers.FindAsync(id);

        if (customer == null)
        {
            return NotFound();
        }

        customer.Name = request.Name;
        customer.Industry = request.Industry;
        customer.ContactName = request.ContactName;
        customer.ContactEmail = request.ContactEmail;
        customer.IsActive = request.IsActive;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CustomerExists(id))
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
    public async Task<IActionResult> DeleteCustomer(Guid id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
        {
            return NotFound();
        }

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool CustomerExists(Guid id)
    {
        return _context.Customers.Any(e => e.Id == id);
    }
}
