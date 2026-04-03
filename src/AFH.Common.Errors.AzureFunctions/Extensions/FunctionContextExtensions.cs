using AFH.Common.Errors.Models;
using Microsoft.Azure.Functions.Worker;

namespace AFH.Common.Errors.AzureFunctions.Extensions;

public static class FunctionContextExtensions
{
    public static ErrorContext ToErrorContext(this FunctionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return new ErrorContext(
            TraceId: context.InvocationId,
            CorrelationId: context.GetCorrelationId(),
            Operation: context.FunctionDefinition?.Name,
            Metadata: new Dictionary<string, string?>
            {
                ["functionId"] = context.FunctionId,
                ["invocationId"] = context.InvocationId
            });
    }

    public static string? GetCorrelationId(this FunctionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Items.TryGetValue("CorrelationId", out var correlationId) && correlationId is not null)
        {
            return correlationId.ToString();
        }

        return null;
    }
}
