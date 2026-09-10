namespace MailForge.Application.DTOs.DomainSearch;

using MailForge.Domain.Enums;

public record DomainSearchRequest(string? Domain = null, string? Company = null, string? JobTitle = null);

public record DomainContactDto(
    string FirstName,
    string LastName,
    string? JobTitle,
    string? Company,
    string Domain,
    string Email,
    EmailStatus Status,
    int ConfidenceScore,
    string Source,
    DateTime? LastVerifiedAt);

public record DomainSearchResponse(
    string Domain,
    string? Company,
    IReadOnlyList<DomainContactDto> Contacts,
    string? ResolvedFrom = null);
