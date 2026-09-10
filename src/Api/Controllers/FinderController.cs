using System.Security.Claims;
using MailForge.Application.DTOs.Finder;
using MailForge.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MailForge.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/finder")]
public class FinderController : ControllerBase
{
    private readonly IFinderService _finderService;
    private readonly IRateLimitService _rateLimitService;

    public FinderController(IFinderService finderService, IRateLimitService rateLimitService)
    {
        _finderService = finderService;
        _rateLimitService = rateLimitService;
    }

    [HttpPost]
    public async Task<ActionResult<FinderResponse>> Find([FromBody] FinderRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        if (!await _rateLimitService.IsAllowedAsync(userId.ToString(), "user", "/api/finder", ct))
            return StatusCode(429, new { error = "Rate limit exceeded" });

        try { return Ok(await _finderService.FindAsync(userId, request, ct)); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

[ApiController]
[Route("api/v1/finder")]
public class PublicFinderController : ControllerBase
{
    private readonly IFinderService _finderService;

    public PublicFinderController(IFinderService finderService) => _finderService = finderService;

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<FinderResponse>> Find([FromBody] FinderRequest request, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try { return Ok(await _finderService.FindAsync(userId, request, ct)); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }
}
