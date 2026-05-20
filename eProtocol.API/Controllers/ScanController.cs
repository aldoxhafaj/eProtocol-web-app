using eProtocol.Application.Abstractions;
using eProtocol.Application.Documents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eProtocol.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator,Manager")]
public sealed class ScanController(IScannerService scannerService, IDocumentService documentService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<DocumentDto>> ScanAndCreate([FromForm] CreateDocumentRequest request, CancellationToken cancellationToken)
    {
        var scannedStream = await scannerService.ScanAsync(cancellationToken);
        var formFile = new StreamFormFile(scannedStream, "scanned_document.pdf", "application/pdf");
        var document = await documentService.CreateAsync(request, formFile, cancellationToken);
        return Ok(document);
    }

    private sealed class StreamFormFile(Stream stream, string fileName, string contentType) : Microsoft.AspNetCore.Http.IFormFile
    {
        public string ContentType => contentType;
        public string ContentDisposition => $"form-data; name=\"file\"; filename=\"{fileName}\"";
        public Microsoft.AspNetCore.Http.IHeaderDictionary Headers => new Microsoft.AspNetCore.Http.HeaderDictionary();
        public long Length => stream.Length;
        public string Name => "file";
        public string FileName => fileName;
        public void CopyTo(Stream target) => stream.CopyTo(target);
        public Task CopyToAsync(Stream target, CancellationToken ct = default) => stream.CopyToAsync(target, ct);
        public Stream OpenReadStream() => stream;
    }
}
