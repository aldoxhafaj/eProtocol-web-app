using eProtocol.Application.Tracking;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eProtocol.API.Controllers;

[ApiController]
[Route("api/tracking")]
[Authorize]
public sealed class TrackingController(ITrackingService trackingService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TrackingAssignmentDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var assignment = await trackingService.GetByIdAsync(id, cancellationToken);
        return assignment is null ? NotFound() : Ok(assignment);
    }

    [HttpPost("{id:guid}/reassign")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Reassign(Guid id, [FromBody] ReassignRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await trackingService.ReassignAsync(id, request, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase) ? NotFound() : Conflict(ex.Message);
        }
    }

    [HttpPut("{id:guid}/deadline")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateDeadline(Guid id, [FromBody] UpdateDeadlineRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await trackingService.UpdateDeadlineAsync(id, request, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase) ? NotFound() : Conflict(ex.Message);
        }
    }

    [HttpPost("{id:guid}/notes")]
    public async Task<ActionResult<AssignmentNoteDto>> AddNote(Guid id, [FromBody] AddNoteRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var note = await trackingService.AddNoteAsync(id, request, cancellationToken);
            return Ok(note);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelAssignmentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await trackingService.CancelAsync(id, request, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase) ? NotFound() : Conflict(ex.Message);
        }
    }

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await trackingService.UpdateStatusAsync(id, request, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase) ? NotFound() : Conflict(ex.Message);
        }
    }
}
