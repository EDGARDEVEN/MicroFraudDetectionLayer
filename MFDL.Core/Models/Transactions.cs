namespace MFDL.Core.Models;

public enum TransactionType
{
    Sale,
    Refund,
    Void,
    Discount,
    DrawerOpen
}

public sealed class Transaction
{
    public string Id { get; init; } = default!;
    public string StoreId { get; init; } = "Store001";
    public string TerminalId { get; init; } = "T001";
    public string CashierId { get; init; } = "Op001";
    public TransactionType Type { get; init; }
    public decimal Amount { get; init; }           // positive values
    public string Currency { get; init; } = "KES";
    public DateTimeOffset Timestamp { get; init; }  // event time
    public Dictionary<string,string> Attributes { get; init; } = new();
}
