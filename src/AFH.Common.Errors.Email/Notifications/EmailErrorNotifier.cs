using AFH.Common.Errors.Abstractions;
using AFH.Common.Errors.Email.Builders;
using AFH.Common.Errors.Email.Models;
using AFH.Common.Errors.Email.Options;
using AFH.Common.Errors.Models;

namespace AFH.Common.Errors.Email.Notifications;

public sealed class EmailErrorNotifier : IErrorNotifier
{
    private readonly ErrorEmailOptions _options;
    private readonly ErrorEmailMessageBuilder _messageBuilder;
    private readonly Func<ErrorEmailTemplateModel, string, CancellationToken, Task> _sender;

    public EmailErrorNotifier(
        ErrorEmailOptions options,
        ErrorEmailMessageBuilder messageBuilder,
        Func<ErrorEmailTemplateModel, string, CancellationToken, Task> sender)
    {
        _options = options;
        _messageBuilder = messageBuilder;
        _sender = sender;
    }

    public Task NotifyAsync(ErrorNotificationRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var model = _messageBuilder.BuildTemplateModel(request, _options);
        var body = _messageBuilder.BuildBody(model);
        return _sender(model, body, cancellationToken);
    }
}
