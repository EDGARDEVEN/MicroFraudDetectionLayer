using MFDL.Core.Models;
using MFDL.Core.Engine;
using MFDL.Core; // To access AlertRepository


var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var repo = new AlertRepository();

// Fetch all alerts
app.MapGet("/alerts", () =>
{
    return repo.GetAllAlerts();
});

// Fetch alerts by operator
app.MapGet("/alerts/operator/{opId}", (string opId) =>
{
    return repo.GetAlertsByOperator(opId);
});

// Fetch alerts in the last N minutes
app.MapGet("/alerts/recent/{minutes}", (int minutes) =>
{
    return repo.GetAlertsSince(TimeSpan.FromMinutes(minutes));
});

app.Run();
