using eProtocol.Application.Documents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace eProtocol.API.Controllers;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class ApiKeyAuthAttribute : Attribute, IAuthorizationFilter
{
    private const string ApiKeyHeaderName = "X-Api-Key";

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var potentialApiKey))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var apiKey = configuration["ApiKey"];

        if (string.IsNullOrEmpty(apiKey) || !string.Equals(apiKey, potentialApiKey, StringComparison.Ordinal))
        {
            context.Result = new UnauthorizedResult();
        }
    }
}

[ApiController]
[Route("api/external/documents")]
[ApiKeyAuth]
public sealed class ExternalDocumentUploadController(IDocumentService documentService) : ControllerBase
{
    [HttpPost("upload")]
    public async Task<ActionResult<DocumentDto>> Upload([FromForm] CreateDocumentRequest request, IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest("File is required.");

        if (FileValidation.ExceedsMaxSize(file.Length))
            return BadRequest($"File size exceeds maximum of {FileValidation.MaxFileSize / (1024 * 1024)} MB.");

        if (!FileValidation.IsValidContentType(file.ContentType))
            return BadRequest("File type is not allowed.");

        var document = await documentService.CreateAsync(request, file, cancellationToken);
        return Ok(document);
    }
}
