namespace AFH.Common.Errors.ApplicationInsights.Telemetry;

public sealed record ErrorTelemetry(
    string Name,
    DateTimeOffset Timestamp,
    IReadOnlyDictionary<string, string?> Properties,
    IReadOnlyDictionary<string, double> Metrics);
