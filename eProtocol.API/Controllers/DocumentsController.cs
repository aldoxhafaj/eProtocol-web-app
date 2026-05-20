using eProtocol.Application.Documents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eProtocol.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class DocumentsController(IDocumentService documentService) : ControllerBase
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

    [HttpPost("{id:guid}/assignments")]
    [Authorize(Roles = "Administrator,Manager")]
    public async Task<IActionResult> Assign(Guid id, [FromBody] AssignDocumentRequest request, CancellationToken cancellationToken)
    {
        await documentService.AssignAsync(id, request, cancellationToken);
        return NoContent();
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
}
