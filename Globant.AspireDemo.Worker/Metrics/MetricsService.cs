using System.Diagnostics.Metrics;

public sealed class MetricsService
{
    public static readonly Meter Meter = new("Todo.Metrics", "1.0");

    private readonly Counter<int> _requestsCounter;
    private readonly Histogram<double> _responseTimeHistogram;

    public MetricsService()
    {
        _requestsCounter = Meter.CreateCounter<int>("todo_requests_total",
            description: "Número total de peticiones recibidas");

        _responseTimeHistogram = Meter.CreateHistogram<double>("todo_response_time_ms",
            description: "Tiempo de respuesta en milisegundos");
    }

    public void IncrementOperation(string operationType)
    {
        _requestsCounter.Add(1, tag: new KeyValuePair<string, object?>("operation", operationType));
    }

    public void RecordDbResponseTime(string operationType, double milliseconds)
    {
        _responseTimeHistogram.Record(milliseconds, tag: new KeyValuePair<string, object?>("operation", operationType));
    }
}
