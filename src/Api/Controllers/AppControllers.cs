using System.Security.Claims;
using MailForge.Application.DTOs.Bulk;
using MailForge.Application.DTOs.Contacts;
using MailForge.Application.DTOs.Dashboard;
using MailForge.Application.DTOs.DomainSearch;
using MailForge.Application.DTOs.Exports;
using MailForge.Application.Interfaces;
using MailForge.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MailForge.Api.Controllers;

[ApiController]
[Authorize]
[Route("api")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    public DashboardController(IDashboardService dashboardService) => _dashboardService = dashboardService;

    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardResponse>> Get(CancellationToken ct) =>
        Ok(await _dashboardService.GetDashboardAsync(GetUserId(), ct));

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

[ApiController]
[Authorize]
[Route("api/domain-search")]
public class DomainSearchController : ControllerBase
{
    private readonly IDomainSearchService _service;
    public DomainSearchController(IDomainSearchService service) => _service = service;

    [HttpPost]
    public async Task<ActionResult<DomainSearchResponse>> Search([FromBody] DomainSearchRequest request, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try { return Ok(await _service.SearchAsync(userId, request, ct)); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }
}

[ApiController]
[Route("api/v1/domain-search")]
public class PublicDomainSearchController : ControllerBase
{
    private readonly IDomainSearchService _service;
    public PublicDomainSearchController(IDomainSearchService service) => _service = service;

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<DomainSearchResponse>> Search([FromBody] DomainSearchRequest request, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try { return Ok(await _service.SearchAsync(userId, request, ct)); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }
}

[ApiController]
[Authorize]
[Route("api/bulk")]
public class BulkController : ControllerBase
{
    private readonly IBulkJobService _service;
    public BulkController(IBulkJobService service) => _service = service;

    [HttpPost]
    [RequestSizeLimit(100_000_000)]
    public async Task<ActionResult<BulkJobCreateResponse>> Upload(IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0) return BadRequest(new { error = "No file uploaded" });
        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = "Only CSV files are supported" });

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await using var stream = file.OpenReadStream();
        return Ok(await _service.CreateJobAsync(userId, stream, file.FileName, ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BulkJobDto>> Get(Guid id, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var job = await _service.GetJobAsync(userId, id, ct);
        return job == null ? NotFound() : Ok(job);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BulkJobDto>>> List(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _service.GetJobsAsync(userId, ct));
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _service.CancelJobAsync(userId, id, ct);
        return NoContent();
    }
}

[ApiController]
[Route("api/v1/bulk")]
public class PublicBulkController : ControllerBase
{
    private readonly IBulkJobService _service;
    public PublicBulkController(IBulkJobService service) => _service = service;

    [Authorize]
    [HttpPost]
    [RequestSizeLimit(100_000_000)]
    public async Task<ActionResult<BulkJobCreateResponse>> Upload(IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0) return BadRequest(new { error = "No file uploaded" });
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await using var stream = file.OpenReadStream();
        return Ok(await _service.CreateJobAsync(userId, stream, file.FileName, ct));
    }

    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BulkJobDto>> Get(Guid id, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var job = await _service.GetJobAsync(userId, id, ct);
        return job == null ? NotFound() : Ok(job);
    }
}

[ApiController]
[Authorize]
[Route("api/contacts")]
public class ContactsController : ControllerBase
{
    private readonly IContactService _service;
    public ContactsController(IContactService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<PagedResult<ContactDto>>> Search([FromQuery] ContactFilterRequest filter, CancellationToken ct) =>
        Ok(await _service.SearchAsync(GetUserId(), filter, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ContactDto>> Get(Guid id, CancellationToken ct)
    {
        var contact = await _service.GetByIdAsync(GetUserId(), id, ct);
        return contact == null ? NotFound() : Ok(contact);
    }

    [HttpPost]
    public async Task<ActionResult<ContactDto>> Save([FromBody] SaveContactRequest request, CancellationToken ct) =>
        Ok(await _service.SaveAsync(GetUserId(), request, ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteAsync(GetUserId(), id, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/tags")]
    public async Task<IActionResult> Tag(Guid id, [FromBody] string tagName, CancellationToken ct)
    {
        await _service.TagContactAsync(GetUserId(), id, tagName, ct);
        return NoContent();
    }

    [HttpPost("deduplicate")]
    public async Task<ActionResult<object>> Deduplicate(CancellationToken ct) =>
        Ok(new { removed = await _service.RemoveDuplicatesAsync(GetUserId(), ct) });

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

[ApiController]
[Authorize]
[Route("api/exports")]
public class ExportsController : ControllerBase
{
    private readonly IExportService _service;
    public ExportsController(IExportService service) => _service = service;

    [HttpPost]
    public async Task<ActionResult<ExportDto>> Create([FromBody] ExportRequest request, CancellationToken ct)
    {
        try { return Ok(await _service.CreateExportAsync(GetUserId(), request, ct)); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ExportHistoryDto>>> History(CancellationToken ct) =>
        Ok(await _service.GetHistoryAsync(GetUserId(), ct));

    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id, [FromQuery] string token, CancellationToken ct)
    {
        var result = await _service.DownloadAsync(GetUserId(), id, token, ct);
        if (result == null) return NotFound();
        return File(result.Value.FileStream, result.Value.ContentType, result.Value.FileName);
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

[ApiController]
[Authorize]
[Route("api/api-keys")]
public class ApiKeysController : ControllerBase
{
    private readonly IApiKeyService _service;
    public ApiKeysController(IApiKeyService service) => _service = service;

    [HttpPost]
    public async Task<ActionResult<object>> Create([FromBody] string name, CancellationToken ct)
    {
        var (rawKey, id) = await _service.CreateKeyAsync(GetUserId(), name, ct);
        return Ok(new { id, key = rawKey, message = "Store this key securely. It won't be shown again." });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Revoke(Guid id, CancellationToken ct)
    {
        await _service.RevokeKeyAsync(GetUserId(), id, ct);
        return NoContent();
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

[ApiController]
[Authorize]
[Route("api/v1")]
public class PublicCreditsController : ControllerBase
{
    private readonly ICreditService _creditService;
    public PublicCreditsController(ICreditService creditService) => _creditService = creditService;

    [HttpGet("credits")]
    public async Task<ActionResult<object>> GetCredits(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(new { balance = await _creditService.GetBalanceAsync(userId, ct) });
    }
}

[ApiController]
[Authorize(Policy = "Admin")]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly MailForgeDbContext _db;
    public AdminController(MailForgeDbContext db) => _db = db;

    [HttpGet("users")]
    public async Task<ActionResult<object>> GetUsers(CancellationToken ct) =>
        Ok(await _db.Users.AsNoTracking().Select(u => new { u.Id, u.Email, u.FirstName, u.LastName, u.Role, u.IsSuspended, u.CreatedAt }).ToListAsync(ct));

    [HttpPost("users/{id:guid}/suspend")]
    public async Task<IActionResult> Suspend(Guid id, CancellationToken ct)
    {
        var user = await _db.Users.FindAsync([id], ct);
        if (user == null) return NotFound();
        user.IsSuspended = true;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpGet("plans")]
    public async Task<ActionResult<object>> GetPlans(CancellationToken ct) =>
        Ok(await _db.Plans.AsNoTracking().ToListAsync(ct));

    [HttpGet("credit-rules")]
    public async Task<ActionResult<object>> GetCreditRules(CancellationToken ct) =>
        Ok(await _db.CreditRules.AsNoTracking().ToListAsync(ct));

    [HttpGet("audit-logs")]
    public async Task<ActionResult<object>> GetAuditLogs(CancellationToken ct) =>
        Ok(await _db.AuditLogs.AsNoTracking().OrderByDescending(a => a.CreatedAt).Take(100).ToListAsync(ct));

    [HttpGet("jobs")]
    public async Task<ActionResult<object>> GetJobs(CancellationToken ct) =>
        Ok(new
        {
            bulk = await _db.BulkJobs.AsNoTracking().OrderByDescending(j => j.CreatedAt).Take(50).ToListAsync(ct),
            search = await _db.SearchJobs.AsNoTracking().OrderByDescending(j => j.CreatedAt).Take(50).ToListAsync(ct)
        });
}
