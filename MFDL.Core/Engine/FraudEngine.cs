using MFDL.Core.Models;
using MFDL.Core.Rules;

namespace MFDL.Core.Engine;

public sealed class FraudEngine
{
    private readonly TransactionHistory _history;
    private readonly List<IFraudRule> _rules = new();

    public FraudEngine(TimeSpan historyRetention)
    {
        _history = new TransactionHistory(historyRetention);
    }

    public FraudEngine RegisterRule(IFraudRule rule)
    {
        _rules.Add(rule);
        return this;
    }

    public IEnumerable<FraudAlert> Process(Transaction t)
    {
        _history.Add(t);
        var recent = _history.Recent(t.Timestamp, TimeSpan.FromMinutes(30)); // working set
        foreach (var rule in _rules)
        {
            var alert = rule.Evaluate(t, recent);
            if (alert != null) yield return alert;
        }
    }
}
