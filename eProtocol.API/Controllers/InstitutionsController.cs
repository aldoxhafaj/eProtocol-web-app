using eProtocol.Application.Institutions;
using Microsoft.AspNetCore.Mvc;

namespace eProtocol.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class InstitutionsController(IInstitutionService institutionService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InstitutionDto>>> GetAll(CancellationToken cancellationToken)
    {
        var institutions = await institutionService.GetAllAsync(cancellationToken);
        return Ok(institutions);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InstitutionDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var institution = await institutionService.GetByIdAsync(id, cancellationToken);
        return institution is null ? NotFound() : Ok(institution);
    }

    [HttpPost]
    public async Task<ActionResult<InstitutionDto>> Create([FromBody] CreateInstitutionRequest request, CancellationToken cancellationToken)
    {
        var institution = await institutionService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = institution.Id }, institution);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<InstitutionDto>> Update(Guid id, [FromBody] UpdateInstitutionRequest request, CancellationToken cancellationToken)
    {
        var institution = await institutionService.UpdateAsync(id, request, cancellationToken);
        return Ok(institution);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await institutionService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
