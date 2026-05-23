using eProtocol.Application.Documents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eProtocol.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class DocumentsController(IDocumentService documentService, IDocumentDeletionPolicy deletionPolicy) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DocumentDto>>> Search([FromQuery] DocumentSearchRequest request, CancellationToken cancellationToken)
    {
        var documents = await documentService.SearchAsync(request, cancellationToken);
        return Ok(documents);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DocumentDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var document = await documentService.GetByIdAsync(id, cancellationToken);
        return document is null ? NotFound() : Ok(document);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator,Manager")]
    public async Task<ActionResult<DocumentDto>> Create([FromForm] CreateDocumentRequest request, IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null)
        {
            return BadRequest("File is required.");
        }

        var document = await documentService.CreateAsync(request, file, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = document.Id }, document);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrator,Manager")]
    public async Task<ActionResult<DocumentDto>> Update(Guid id, [FromForm] UpdateDocumentRequest request, IFormFile? file, CancellationToken cancellationToken)
    {
        try
        {
            var document = await documentService.UpdateAsync(id, request, file, cancellationToken);
            return Ok(document);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpGet("{id:guid}/file")]
    public async Task<IActionResult> DownloadFile(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await documentService.GetFileAsync(id, cancellationToken);
            if (result is null)
            {
                return NotFound();
            }

            return File(result.Content, result.ContentType, result.FileName);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPost("{id:guid}/assignments")]
    [Authorize(Roles = "Administrator,Manager")]
    public async Task<IActionResult> Assign(Guid id, [FromBody] AssignDocumentRequest request, CancellationToken cancellationToken)
    {
        await documentService.AssignAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}/assignments")]
    public async Task<ActionResult<IReadOnlyList<DocumentAssignmentDto>>> GetAssignments(Guid id, CancellationToken cancellationToken)
    {
        var assignments = await documentService.GetAssignmentsAsync(id, cancellationToken);
        return assignments is null ? NotFound() : Ok(assignments);
    }

    [HttpDelete("{id:guid}/assignments/{assignmentId:guid}")]
    [Authorize(Roles = "Administrator,Manager")]
    public async Task<IActionResult> RemoveAssignment(Guid id, Guid assignmentId, CancellationToken cancellationToken)
    {
        var removed = await documentService.RemoveAssignmentAsync(id, assignmentId, cancellationToken);
        return removed ? Ok() : NotFound();
    }

    [HttpGet("my-assignments")]
    public async Task<ActionResult<IReadOnlyList<DocumentDto>>> GetMyAssignments(CancellationToken cancellationToken)
    {
        var documents = await documentService.GetMyAssignmentsAsync(cancellationToken);
        return Ok(documents);
    }

    [HttpPost("assignments/{assignmentId:guid}/complete")]
    public async Task<IActionResult> CompleteAssignment(Guid assignmentId, CancellationToken cancellationToken)
    {
        await documentService.CompleteAssignmentAsync(assignmentId, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrator,Manager,Employee")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await deletionPolicy.EvaluateAsync(id, cancellationToken);
        if (!result.Allowed)
        {
            return result.StatusCode switch
            {
                404 => NotFound(result.Reason),
                403 => StatusCode(403, result.Reason),
                409 => Conflict(result.Reason),
                _ => BadRequest(result.Reason)
            };
        }

        await deletionPolicy.ExecuteDeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/archive")]
    [Authorize(Roles = "Administrator,Manager")]
    public async Task<IActionResult> Archive(Guid id, CancellationToken cancellationToken)
    {
        await documentService.ArchiveAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/unarchive")]
    [Authorize(Roles = "Administrator,Manager")]
    public async Task<IActionResult> Unarchive(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await documentService.UnarchiveAsync(id, cancellationToken);
            return Ok();
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }
}
