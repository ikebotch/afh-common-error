using AFH.Common.Errors.Models;

namespace AFH.Common.Errors.Abstractions;

public interface IErrorNotifier
{
    Task NotifyAsync(ErrorNotificationRequest request, CancellationToken cancellationToken = default);
}
