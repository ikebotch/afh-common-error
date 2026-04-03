namespace AFH.Common.Errors.ApplicationInsights.Telemetry;

public static class ErrorTelemetryConstants
{
    public const string DefaultTelemetryName = "afh.common_errors";

    public static class PropertyKeys
    {
        public const string ErrorCode = "afh.error.code";
        public const string ErrorCategory = "afh.error.category";
        public const string ErrorSeverity = "afh.error.severity";
        public const string ErrorMessage = "afh.error.message";
        public const string ErrorExceptionType = "afh.error.exception_type";
        public const string ErrorTraceId = "afh.error.trace_id";
        public const string ErrorCorrelationId = "afh.error.correlation_id";
        public const string ErrorOperation = "afh.error.operation";
        public const string ErrorPath = "afh.error.path";
        public const string ErrorMethod = "afh.error.method";
        public const string ErrorUserId = "afh.error.user_id";
    }

    public static class MetricKeys
    {
        public const string DetailsCount = "afh.error.details_count";
    }
}
