using System.Text;
using AFH.Common.Errors.Email.Models;
using AFH.Common.Errors.Email.Options;
using AFH.Common.Errors.Models;

namespace AFH.Common.Errors.Email.Builders;

public sealed class ErrorEmailMessageBuilder
{
    public ErrorEmailTemplateModel BuildTemplateModel(ErrorNotificationRequest request, ErrorEmailOptions options)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(options);

        return new ErrorEmailTemplateModel
        {
            Subject = BuildSubject(request, options),
            Summary = request.Summary,
            Severity = request.Severity,
            ErrorCode = request.Record.Code,
            Category = request.Record.Category,
            OccurredUtc = request.Record.OccurredUtc,
            TraceId = request.Record.Context?.TraceId,
            CorrelationId = request.Record.Context?.CorrelationId,
            ExceptionType = request.Record.ExceptionType,
            Details = options.IncludeDetails ? request.Record.Details : [],
            Metadata = request.Metadata,
            ToAddresses = options.ToAddresses,
            CcAddresses = options.CcAddresses,
            BccAddresses = options.BccAddresses,
            FromAddress = options.FromAddress,
            FromDisplayName = options.FromDisplayName
        };
    }

    public string BuildSubject(ErrorNotificationRequest request, ErrorEmailOptions options)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(options);

        return $"{options.SubjectPrefix} {request.Severity}: {request.Record.Code}";
    }

    public string BuildBody(ErrorEmailTemplateModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var builder = new StringBuilder();
        builder.AppendLine(model.Subject);
        builder.AppendLine();
        builder.AppendLine($"Summary: {model.Summary}");
        builder.AppendLine($"Severity: {model.Severity}");
        builder.AppendLine($"Code: {model.ErrorCode}");
        builder.AppendLine($"Category: {model.Category}");
        builder.AppendLine($"OccurredUtc: {model.OccurredUtc:O}");

        if (!string.IsNullOrWhiteSpace(model.TraceId))
        {
            builder.AppendLine($"TraceId: {model.TraceId}");
        }

        if (!string.IsNullOrWhiteSpace(model.CorrelationId))
        {
            builder.AppendLine($"CorrelationId: {model.CorrelationId}");
        }

        if (!string.IsNullOrWhiteSpace(model.ExceptionType))
        {
            builder.AppendLine($"ExceptionType: {model.ExceptionType}");
        }

        if (model.Details.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Details:");

            foreach (var detail in model.Details)
            {
                builder.AppendLine($"- {detail.Code}: {detail.Message}");
            }
        }

        if (model.Metadata.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Metadata:");

            foreach (var pair in model.Metadata.OrderBy(pair => pair.Key, StringComparer.Ordinal))
            {
                builder.AppendLine($"- {pair.Key}: {pair.Value}");
            }
        }

        return builder.ToString().TrimEnd();
    }
}
