using MailForge.Domain.Enums;

namespace MailForge.Application.DTOs.Contacts;

public record ContactDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? JobTitle,
    string? Company,
    string? Domain,
    string? Country,
    string? Industry,
    EmailStatus Status,
    int ConfidenceScore,
    string? Source,
    DateTime? LastVerifiedAt,
    DateTime CreatedAt,
    IReadOnlyList<string> Tags);

public record SaveContactRequest(
    string FirstName,
    string LastName,
    string Email,
    string? JobTitle = null,
    string? Company = null,
    string? Domain = null,
    EmailStatus Status = EmailStatus.Unknown,
    int ConfidenceScore = 0,
    string? Source = null);

public record ContactFilterRequest(
    string? Company = null,
    string? Domain = null,
    string? JobTitle = null,
    EmailStatus? Status = null,
    string? Country = null,
    string? Industry = null,
    int? MinConfidence = null,
    DateTime? CreatedAfter = null,
    int Page = 1,
    int PageSize = 20);

public record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);
