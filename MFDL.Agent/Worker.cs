using MFDL.Adapters;
using MFDL.Core;

namespace MFDL.Agent;

public class Worker
{
    private readonly IPosAdapter _adapter;

    public Worker(IPosAdapter adapter)
    {
        _adapter = adapter;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        await foreach (var ev in _adapter.ReadAsync(ct))
        {
            Console.WriteLine($"[EVENT] {ev.EventType} {ev.TransactionId} {ev.Amount} {ev.OperatorId}");

            if (ev.EventType == "Refund" && ev.Amount > 500)
            {
                var alert = new Alert(
                    Guid.NewGuid().ToString("N"),
                    "R001",
                    ev.StoreId,
                    ev.OperatorId,
                    ev.TransactionId,
                    DateTimeOffset.UtcNow,
                    85,
                    "High",
                    $"Suspicious refund detected for {ev.Amount} by {ev.OperatorId}",
                    new List<string>(),
                    new Dictionary<string, object> { { "Amount", ev.Amount } }
                );

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ALERT] {alert.Explanation}");
                Console.ResetColor();
            }
        }
    }
}
