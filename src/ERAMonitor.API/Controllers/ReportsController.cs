using System.Security.Claims;
using ERAMonitor.Core.DTOs.Common;
using ERAMonitor.Core.DTOs.Reports;
using ERAMonitor.Core.Enums;
using ERAMonitor.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERAMonitor.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<ReportDto>>> GetPaged([FromQuery] PagedRequest request, [FromQuery] ReportType? type = null)
    {
        var orgId = Guid.Parse(User.FindFirst("OrganizationId")?.Value ?? throw new UnauthorizedAccessException());
        
        var result = await _reportService.GetPagedAsync(orgId, request, type);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ReportDetailDto>> GetDetail(Guid id)
    {
        var orgId = Guid.Parse(User.FindFirst("OrganizationId")?.Value ?? throw new UnauthorizedAccessException());

        var result = await _reportService.GetDetailAsync(id, orgId);
        if (result == null) return NotFound();
        
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ReportDetailDto>> Create(CreateReportDto dto)
    {
        var orgId = Guid.Parse(User.FindFirst("OrganizationId")?.Value ?? throw new UnauthorizedAccessException());

        var result = await _reportService.CreateAsync(orgId, dto);
        return CreatedAtAction(nameof(GetDetail), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ReportDetailDto>> Update(Guid id, UpdateReportDto dto)
    {
        var orgId = Guid.Parse(User.FindFirst("OrganizationId")?.Value ?? throw new UnauthorizedAccessException());

        var result = await _reportService.UpdateAsync(id, orgId, dto);
        if (result == null) return NotFound();

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var orgId = Guid.Parse(User.FindFirst("OrganizationId")?.Value ?? throw new UnauthorizedAccessException());

        var result = await _reportService.DeleteAsync(id, orgId);
        if (!result) return NotFound();

        return NoContent();
    }

    [HttpPost("{id}/generate")]
    public async Task<ActionResult<ReportExecutionDto>> Generate(Guid id)
    {
        var orgId = Guid.Parse(User.FindFirst("OrganizationId")?.Value ?? throw new UnauthorizedAccessException());
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException());

        try
        {
            var result = await _reportService.GenerateAsync(id, orgId, userId);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("{id}/executions")]
    public async Task<ActionResult<PagedResponse<ReportExecutionDto>>> GetExecutions(Guid id, [FromQuery] PagedRequest request)
    {
        var orgId = Guid.Parse(User.FindFirst("OrganizationId")?.Value ?? throw new UnauthorizedAccessException());

        var result = await _reportService.GetExecutionsAsync(id, orgId, request);
        return Ok(result);
    }

    [HttpGet("executions/{executionId}/download")]
    public async Task<ActionResult> DownloadExecution(Guid executionId)
    {
        var orgId = Guid.Parse(User.FindFirst("OrganizationId")?.Value ?? throw new UnauthorizedAccessException());

        try
        {
            var bytes = await _reportService.DownloadExecutionAsync(executionId, orgId);
            if (bytes.Length == 0) return NotFound("File not found or empty");

            return File(bytes, "application/octet-stream", $"report_{executionId}.pdf"); // Default name/type
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
