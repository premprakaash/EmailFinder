using MailForge.Domain.Enums;

namespace MailForge.Application.DTOs.DomainSearch;

public record DomainSearchRequest(string Domain);

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

public record DomainSearchResponse(string Domain, IReadOnlyList<DomainContactDto> Contacts);
