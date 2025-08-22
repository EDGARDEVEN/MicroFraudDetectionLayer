using MFDL.Core.Models;
using MFDL.Core.Rules;

namespace MFDL.Core.Engine
{
    public sealed class FraudEngine
    {
        private readonly TransactionHistory _history;
        private readonly List<IFraudRule> _rules = new();
        private readonly AlertRepository _repository;

        public FraudEngine(TimeSpan historyRetention, string dbPath = "alerts.db")
        {
            _history = new TransactionHistory(historyRetention);
            _repository = new AlertRepository(dbPath);
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
                if (alert != null)
                {
                    // Save to repository before returning
                    _repository.SaveAlert(alert);
                    yield return alert;
                }
            }
        }
    }
}
