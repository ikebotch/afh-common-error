using AFH.Common.Errors.Models;

namespace AFH.Common.Errors.ApplicationInsights.Telemetry;

public sealed class ErrorTelemetryBuilder
{
    private readonly ErrorTelemetryMapper _mapper;
    private readonly ErrorTelemetryEnricher _enricher;

    public ErrorTelemetryBuilder(ErrorTelemetryMapper mapper, ErrorTelemetryEnricher enricher)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _enricher = enricher ?? throw new ArgumentNullException(nameof(enricher));
    }

    public ErrorTelemetry Build(ErrorRecord record, Action<IDictionary<string, string?>, IDictionary<string, double>>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(record);

        var telemetry = _mapper.Map(record);
        var properties = new Dictionary<string, string?>(telemetry.Properties, StringComparer.Ordinal);
        var metrics = new Dictionary<string, double>(telemetry.Metrics, StringComparer.Ordinal);

        configure?.Invoke(properties, metrics);
        _enricher.Enrich(record, properties, metrics);

        return telemetry with { Properties = properties, Metrics = metrics };
    }
}
