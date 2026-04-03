using AFH.Common.Errors.Models;

namespace AFH.Common.Errors.Builders;

public sealed class NotificationRequestBuilder
{
    public ErrorNotificationRequest Build(ErrorRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        return new ErrorNotificationRequest
        {
            Subject = $"{record.Severity}: {record.Code}",
            Summary = record.Message,
            Severity = record.Severity,
            Record = record,
            Metadata = record.Context?.Metadata ?? new Dictionary<string, string?>()
        };
    }
}
