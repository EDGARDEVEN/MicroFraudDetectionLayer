using MFDL.Core.Models;

namespace MFDL.Core.Rules;

/// <summary>
/// Flags excessive refunds per cashier in a short window
/// and single unusually large refunds.
/// </summary>
public sealed class RefundRule : IFraudRule
{
    public string Id => "R001_RefundBurst";
    public string Name => "Refund Burst / Large Refund";

    private readonly TimeSpan _window = TimeSpan.FromMinutes(15);
    private readonly int _countThreshold = 3;          // >3 refunds in 15 min
    private readonly decimal _singleRefundAmount = 10000m; // KES 10,000

    public FraudAlert? Evaluate(Transaction current, IReadOnlyList<Transaction> recent)
    {
        if (current.Type != TransactionType.Refund) return null;

        // Condition 1: large single refund
        if (current.Amount >= _singleRefundAmount)
        {
            return new FraudAlert
            {
                Rule = Name,
                Severity = AlertSeverity.High,
                Message = $"Large refund: {current.Amount:n0} by {current.CashierId}",
                StoreId = current.StoreId,
                CashierId = current.CashierId,
                TransactionId = current.Id,
                Features = new() {
                    ["type"] = "single_large_refund",
                    ["amount"] = current.Amount
                }
            };
        }

        // Condition 2: many refunds by same cashier in rolling window
        var since = current.Timestamp - _window;
        var byCashier = recent.Where(t =>
                t.CashierId == current.CashierId &&
                t.Type == TransactionType.Refund &&
                t.Timestamp >= since &&
                t.Timestamp <= current.Timestamp)
            .ToList();

        if (byCashier.Count > _countThreshold)
        {
            var total = byCashier.Sum(t => t.Amount);
            return new FraudAlert
            {
                Rule = Name,
                Severity = AlertSeverity.High,
                Message = $"Refund burst: {byCashier.Count} refunds (~{total:n0}) in {_window.TotalMinutes}min by {current.CashierId}",
                StoreId = current.StoreId,
                CashierId = current.CashierId,
                TransactionId = current.Id,
                Features = new() {
                    ["count_window"] = _window.TotalMinutes,
                    ["count"] = byCashier.Count,
                    ["sum"] = total
                }
            };
        }

        return null;
    }
}
// This rule detects excessive refunds by a cashier within a short time frame
// and flags large single refunds, generating alerts for potential fraud.