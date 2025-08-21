using MFDL.Core.Models;

namespace MFDL.Core.Engine;

/// <summary>
/// Rolling, in-memory history with time-based eviction.
/// Thread-safe for simple reads/writes.
/// </summary>
public sealed class TransactionHistory
{
    private readonly TimeSpan _retention;
    private readonly LinkedList<Transaction> _events = new();
    private readonly object _lock = new();

    public TransactionHistory(TimeSpan retention)
    {
        _retention = retention;
    }

    public void Add(Transaction t)
    {
        lock (_lock)
        {
            _events.AddLast(t);
            EvictOld(t.Timestamp);
        }
    }

    public IReadOnlyList<Transaction> Recent(DateTimeOffset asOf, TimeSpan window) 
    {
        var cutoff = asOf - window;
        lock (_lock)
        {
            var list = new List<Transaction>();
            for (var node = _events.Last; node != null; node = node.Previous)
            {
                if (node.Value.Timestamp < cutoff) break;
                list.Add(node.Value);
            }
            list.Reverse();
            return list;
        }
    }

    private void EvictOld(DateTimeOffset latestTs)
    {
        var cutoff = latestTs - _retention;
        while (_events.First is { } head && head.Value.Timestamp < cutoff)
            _events.RemoveFirst();
    }
}
// This class provides a thread-safe, rolling history of transactions with time-based eviction.