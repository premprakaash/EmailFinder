using MailForge.Domain.Enums;

namespace MailForge.Application.DTOs.Finder;

public record FinderRequest(string FirstName, string LastName, string Domain, string? Company = null, string? JobTitle = null);

public record FinderCandidateDto(
    string Email,
    int Confidence,
    EmailStatus Status,
    string Pattern);

public record FinderResponse(
    string FirstName,
    string LastName,
    string Domain,
    IReadOnlyList<FinderCandidateDto> Candidates,
    FinderCandidateDto? BestMatch);
