using System.Security.Claims;
using MailForge.Application.DTOs.Verifier;
using MailForge.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MailForge.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/verifier")]
public class VerifierController : ControllerBase
{
    private readonly IVerifierService _verifierService;

    public VerifierController(IVerifierService verifierService) => _verifierService = verifierService;

    [HttpPost]
    public async Task<ActionResult<VerifyResponse>> Verify([FromBody] VerifyRequest request, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try { return Ok(await _verifierService.VerifyAsync(userId, request, ct)); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }
}

[ApiController]
[Route("api/v1/verifier")]
public class PublicVerifierController : ControllerBase
{
    private readonly IVerifierService _verifierService;

    public PublicVerifierController(IVerifierService verifierService) => _verifierService = verifierService;

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<VerifyResponse>> Verify([FromBody] VerifyRequest request, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try { return Ok(await _verifierService.VerifyAsync(userId, request, ct)); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }
}
