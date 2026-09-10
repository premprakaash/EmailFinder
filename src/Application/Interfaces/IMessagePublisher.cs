namespace MailForge.Application.Interfaces;

public interface IMessagePublisher
{
    Task PublishAsync<T>(string queueName, T message, CancellationToken cancellationToken = default);
}

public record BulkJobMessage(Guid JobId, Guid UserId);
public record ExportJobMessage(Guid ExportId, Guid UserId);
public record VerificationJobMessage(Guid JobId, Guid UserId, string Email);
public record FinderJobMessage(Guid UserId, string FirstName, string LastName, string Domain);
