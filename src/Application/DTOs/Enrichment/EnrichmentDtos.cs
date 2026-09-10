namespace MailForge.Application.DTOs.Enrichment;

public record EnrichmentRequest(string Email);

public record EnrichmentResponse(
    string Email,
    string? FirstName,
    string? LastName,
    string? JobTitle,
    string? Company,
    string? Domain,
    string? Country,
    string? Industry,
    string Source);

public record PeopleDiscoveryRequest(string? Company = null, string? Domain = null, string? JobTitle = null);

public record PeopleDiscoveryResponse(
    string QueryType,
    string? Company,
    string Domain,
    IReadOnlyList<DomainSearch.DomainContactDto> Contacts,
    string? ResolvedFrom);
