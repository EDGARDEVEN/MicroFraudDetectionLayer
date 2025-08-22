using MFDL.Core.Engine;
using MFDL.Core.Models;
using MFDL.Core.Rules;
using MFDL.Core; // for AlertRepository

static class Program
{
    static async Task Main()
    {
        Console.WriteLine(" MFDL – MicroFraudDetectionEngine \n");

        
        var repo = new AlertRepository();

        var engine = new FraudEngine(TimeSpan.FromHours(2), "alerts.db")
            .RegisterRule(new RefundRule())
            .RegisterRule(new VelocityRule());

        // Generate mock async stream
        var now = DateTimeOffset.UtcNow;
        var rand = new Random(42);

        await foreach (var tx in GenerateMockTransactionsAsync(now, rand))
        {
            foreach (var alert in engine.Process(tx))
            {
                PrintAlert(alert);
                // alerts already saved to DB inside FraudEngine
            }

            await Task.Delay(50); // simulate real-time arrival
        }

        Console.WriteLine("\n Analysis complete. Alerts saved to alerts.db");
    }

    /// <summary>
    /// Generates synthetic transactions for testing
    /// </summary>
    static async IAsyncEnumerable<Transaction> GenerateMockTransactionsAsync(DateTimeOffset start, Random rng)
    {
        var t = start;

        // cashier A: normal with a refund burst later
        for (int i = 0; i < 120; i++)
        {
            yield return NewTxn($"TXN{i:0000}", "Store001", "T1", "OpA",
                TransactionType.Sale, AmountRandom(rng, 100, 1500), t);
            t = t.AddSeconds(rng.Next(5, 15));
            await Task.Yield(); // yield control for async
        }

        // Refund burst for OpA
        for (int i = 0; i < 5; i++)
        {
            yield return NewTxn($"TXN_R{i:000}", "Store001", "T1", "OpA",
                TransactionType.Refund, AmountRandom(rng, 200, 2000), t);
            t = t.AddSeconds(rng.Next(10, 60));
            await Task.Yield();
        }

        // Large single refund
        yield return NewTxn("TXN_BIGR", "Store001", "T1", "OpA",
            TransactionType.Refund, 15000m, t);
        t = t.AddSeconds(30);
        await Task.Yield();

        // OpB ultra-fast transactions (velocity anomaly)
        for (int i = 0; i < 80; i++)
        {
            var type = rng.NextDouble() < 0.95 ? TransactionType.Sale : TransactionType.Refund;
            yield return NewTxn($"TXN_B{i:0000}", "Store001", "T2", "OpB",
                type, AmountRandom(rng, 50, 800), t);
            t = t.AddSeconds(2); // very fast
            await Task.Yield();
        }
    }

    static Transaction NewTxn(string id, string store, string term, string cashier,
        TransactionType type, decimal amount, DateTimeOffset ts)
        => new Transaction
        {
            Id = id,
            StoreId = store,
            TerminalId = term,
            CashierId = cashier,
            Type = type,
            Amount = amount,
            Timestamp = ts
        };

    static decimal AmountRandom(Random rng, int min, int max)
        => rng.Next(min, max);

    static void PrintAlert(FraudAlert a)
    {
        var color = a.Severity switch
        {
            AlertSeverity.High => ConsoleColor.Red,
            AlertSeverity.Medium => ConsoleColor.Yellow,
            _ => ConsoleColor.Gray
        };
        Console.ForegroundColor = color;
        Console.WriteLine($"[ALERT] {a.Rule} | {a.Severity} | {a.Message}");
        Console.ResetColor();
    }
}
