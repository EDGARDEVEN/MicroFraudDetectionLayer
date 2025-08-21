namespace MFDL.Core.Models;

public enum AlertSeverity { Low, Medium, High }

public sealed class FraudAlert
{
    public string AlertId { get; init; } = Guid.NewGuid().ToString("N");
    public string Rule { get; init; } = default!;
    public AlertSeverity Severity { get; init; }
    public string Message { get; init; } = default!;
    public string StoreId { get; init; } = default!;
    public string CashierId { get; init; } = default!;
    public string TransactionId { get; init; } = default!;
    public DateTimeOffset DetectedAt { get; init; } = DateTimeOffset.UtcNow;
    public Dictionary<string,object> Features { get; init; } = new();
}
