using MFDL.Core;

namespace MFDL.Adapters;

public interface IPosAdapter
{
    IAsyncEnumerable<PosEvent> ReadAsync(CancellationToken ct);
}
