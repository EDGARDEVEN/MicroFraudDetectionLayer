using MFDL.Core;

namespace MFDL.Adapters;

public class MockAdapter : IPosAdapter
{
    public async IAsyncEnumerable<PosEvent> ReadAsync([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        for (int i = 0; i < 50; i++)
        {
            yield return new PosEvent(
                "MockDB", "Store001", "T001", "Op123",
                i % 5 == 0 ? "Refund" : "Sale",
                DateTimeOffset.UtcNow.AddMinutes(-i),
                $"TXN{i:D4}",
                i % 5 == 0 ? 1000 : 200,
                "KES",
                "Cash",
                new Dictionary<string,string>()
            );
            await Task.Delay(200, ct);
        }
    }
}
