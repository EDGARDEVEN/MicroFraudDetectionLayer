# Micro Fraud Detection Layer (MFDL)

## Overview
The Micro Fraud Detection Layer (MFDL) is a lightweight, pluggable fraud detection engine written in **C# (.NET 8)**.  
It is designed to be integrated into business systems such as **POS terminals, e-commerce platforms, banking systems, or mobile money applications** as a **real-time rule-based microservice**.

Instead of being a large monolithic fraud system, MFDL acts as a **middleware layer** between transaction flows and business logic - quietly scanning transactions, applying detection rules, and raising alerts when suspicious behavior is found.

---

## Features
- ✅ Rule-based fraud detection engine
- ✅ Modular and extensible (plug in new rules easily)
- ✅ Lightweight and fast (C# .NET 8)
- ✅ Works as a library or standalone console
- ✅ Mock dataset for demonstration

### Current Rules Implemented
1. **RefundRule** – Flags suspicious refund transactions above a configurable threshold.
2. **VelocityRule** – Detects multiple high-value transactions within a short time window.

---

## Getting Started

### 1. Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- Linux/Mac/Windows

### 2. Build
```bash
cd MFDL
dotnet build
```

### 3. Run Demo
```bash
dotnet run --project MFDL.Tools
```
This will run the **FraudEngine** against sample mock transactions and print suspicious activity to the console.

---

### Using Docker
You can also run the demo using [Docker](https://docs.docker.com/desktop/):
```bash
docker build -t mfdldemo:latest .
docker run --rm mfdldemo
```
---

## Example Output
```
[ALERT] Refund transaction T1003 flagged: suspicious refund detected!
[ALERT] Velocity breach by User U123: 3 high-value txns within 2 mins!
```

---

## How to Extend

### Adding a New Rule
1. Create a new rule class in `MFDL.Core/Rules/` implementing `IFraudRule`.
2. Define `Evaluate(Transaction txn, IEnumerable<Transaction> history)`.
3. Add the rule in `FraudEngine.cs`.

Example:
```csharp
public class GeoLocationRule : IFraudRule
{
    public string Name => "GeoLocation Rule";

    public bool Evaluate(Transaction txn, IEnumerable<Transaction> history)
    {
        // Example: flag if two txns happen from distant locations in <10 mins
        return false;
    }
}
```

---

## Real-World Application Ideas
- 🔹 **Banking** – detect account takeover or rapid ATM withdrawals.  
- 🔹 **E-commerce** – detect abnormal refunds or reselling fraud.  
- 🔹 **Mobile Money** – detect SIM swap or cash-out velocity abuse.  
- 🔹 **POS systems** – prevent internal employee refund scams.

---

## Next Steps
- [ ] Add more detection rules (geo-location, device fingerprinting, merchant risk scoring)
- [ ] Package as a **NuGet library**
- [ ] Wrap with **gRPC/REST API microservice** for real integrations
- [ ] Persist alerts in a database (e.g., PostgreSQL, MongoDB)

---

## Author
Developed by **Deven Cybersecurity Services (DCS)**  
For more information, email me at [edgardeven303@gmail.com](mailto:edgardeven303@gmail.com).
