using MFDL.Core.Models;

namespace MFDL.Core.Rules;

public interface IFraudRule
{
    string Id { get; }
    string Name { get; }
    FraudAlert? Evaluate(Transaction current, IReadOnlyList<Transaction> recent);
}
