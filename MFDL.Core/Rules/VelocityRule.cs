using MFDL.Core.Models;

namespace MFDL.Core.Rules;

/// <summary>
/// Cashier velocity: too many transactions/minute sustained in short window.
/// Flags robotic speed or suspicious batching.
/// </summary>
public sealed class VelocityRule : IFraudRule
{
    public string Id => "R002_Velocity";
    public string Name => "Cashier Velocity Anomaly";

    private readonly TimeSpan _window = TimeSpan.FromMinutes(5);
    private readonly int _txnThreshold = 60; // >60 txns in 5 minutes (~12 TPM)

    public FraudAlert? Evaluate(Transaction current, IReadOnlyList<Transaction> recent)
    {
        var since = current.Timestamp - _window;
        var byCashier = recent.Where(t =>
                t.CashierId == current.CashierId &&
                t.Timestamp >= since &&
                t.Timestamp <= current.Timestamp)
            .ToList();

        if (byCashier.Count > _txnThreshold)
        {
            var tpm = byCashier.Count / (_window.TotalMinutes);
            return new FraudAlert
            {
                Rule = Name,
                Severity = tpm > 20 ? AlertSeverity.High : AlertSeverity.Medium,
                Message = $"Unusual speed: {byCashier.Count} txns in {_window.TotalMinutes}min (~{tpm:F1} TPM) by {current.CashierId}",
                StoreId = current.StoreId,
                CashierId = current.CashierId,
                TransactionId = current.Id,
                Features = new() {
                    ["window_min"] = _window.TotalMinutes,
                    ["count"] = byCashier.Count,
                    ["tpm"] = Math.Round(tpm,1)
                }
            };
        }

        return null;
    }
}
// This rule detects excessive transaction velocity by a cashier within a short time frame
// and generates alerts for potential fraud, indicating robotic speed or suspicious batching.