using Microsoft.Data.Sqlite;
using System.IO;
using MFDL.Core.Models;

namespace MFDL.Core
{
    public class AlertRepository
    {
        private readonly string _dbPath;

        public AlertRepository(string? dbPath = null)
        {
            // Default to solution root: ~/Desktop/MFDL/alerts.db
            var basePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
            _dbPath = dbPath ?? Path.Combine(basePath, "alerts.db");

            Console.WriteLine($"[DB] Using database at: {_dbPath}");
            
            // Ensure directory exists
            
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            if (!File.Exists(_dbPath))
            {
                using var connection = new SqliteConnection($"Data Source={_dbPath}");
                connection.Open();

                var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                CREATE TABLE FraudAlerts (
                    AlertId TEXT PRIMARY KEY,
                    Rule TEXT NOT NULL,
                    Severity TEXT NOT NULL,
                    Message TEXT NOT NULL,
                    StoreId TEXT NOT NULL,
                    CashierId TEXT NOT NULL,
                    TransactionId TEXT NOT NULL,
                    DetectedAt TEXT NOT NULL
                );";
                cmd.ExecuteNonQuery();
            }
        }

        public void SaveAlert(FraudAlert alert)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
            INSERT INTO FraudAlerts 
            (AlertId, Rule, Severity, Message, StoreId, CashierId, TransactionId, DetectedAt)
            VALUES ($id, $rule, $sev, $msg, $store, $cashier, $tx, $time)";
            
            cmd.Parameters.AddWithValue("$id", alert.AlertId);
            cmd.Parameters.AddWithValue("$rule", alert.Rule);
            cmd.Parameters.AddWithValue("$sev", alert.Severity.ToString()); 
            cmd.Parameters.AddWithValue("$msg", alert.Message);
            cmd.Parameters.AddWithValue("$store", alert.StoreId);
            cmd.Parameters.AddWithValue("$cashier", alert.CashierId);
            cmd.Parameters.AddWithValue("$tx", alert.TransactionId);
            cmd.Parameters.AddWithValue("$time", alert.DetectedAt.ToString("o")); 

            cmd.ExecuteNonQuery();
        }

        public IEnumerable<FraudAlert> GetAllAlerts()
        {
            var alerts = new List<FraudAlert>();
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM FraudAlerts ORDER BY DetectedAt DESC";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                alerts.Add(MapReaderToAlert(reader));
            }
            return alerts;
        }

        public FraudAlert? GetAlertById(string id)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM FraudAlerts WHERE AlertId = $id";
            cmd.Parameters.AddWithValue("$id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return MapReaderToAlert(reader);
            }
            return null;
        }

        // ✅ NEW: Get alerts by Cashier (Operator)
        public IEnumerable<FraudAlert> GetAlertsByOperator(string cashierId)
        {
            var alerts = new List<FraudAlert>();
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM FraudAlerts WHERE CashierId = $cashier ORDER BY DetectedAt DESC";
            cmd.Parameters.AddWithValue("$cashier", cashierId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                alerts.Add(MapReaderToAlert(reader));
            }
            return alerts;
        }

        // ✅ NEW: Get alerts by Store
        public IEnumerable<FraudAlert> GetAlertsByStore(string storeId)
        {
            var alerts = new List<FraudAlert>();
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM FraudAlerts WHERE StoreId = $store ORDER BY DetectedAt DESC";
            cmd.Parameters.AddWithValue("$store", storeId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                alerts.Add(MapReaderToAlert(reader));
            }
            return alerts;
        }

        // ✅ Fetch alerts by Severity (Critical, High, Medium, Low, etc.)
        public IEnumerable<FraudAlert> GetAlertsBySeverity(AlertSeverity severity)
        {
            var alerts = new List<FraudAlert>();
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM FraudAlerts WHERE Severity = $sev ORDER BY DetectedAt DESC";
            cmd.Parameters.AddWithValue("$sev", severity.ToString());

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                alerts.Add(MapReaderToAlert(reader));
            }
            return alerts;
        }

        // ✅ Fetch alerts within the last N minutes/hours/days
                // ✅ Fetch alerts within the last N minutes/hours/days
        public IEnumerable<FraudAlert> GetAlertsSince(TimeSpan timeWindow)
        {
            var alerts = new List<FraudAlert>();
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            var cutoff = DateTime.UtcNow - timeWindow;

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM FraudAlerts WHERE DetectedAt >= $cutoff ORDER BY DetectedAt DESC";
            cmd.Parameters.AddWithValue("$cutoff", cutoff.ToString("o")); // store & compare in ISO8601

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                alerts.Add(MapReaderToAlert(reader));
            }
            return alerts;
        }


        // 🔹 Helper method to avoid duplication
        private FraudAlert MapReaderToAlert(SqliteDataReader reader)
        {
            return new FraudAlert
            {
                AlertId = reader.GetString(0),
                Rule = reader.GetString(1),
                Severity = Enum.Parse<AlertSeverity>(reader.GetString(2)),
                Message = reader.GetString(3),
                StoreId = reader.GetString(4),
                CashierId = reader.GetString(5),
                TransactionId = reader.GetString(6),
                DetectedAt = DateTime.Parse(reader.GetString(7))
            };
        }
    }
}