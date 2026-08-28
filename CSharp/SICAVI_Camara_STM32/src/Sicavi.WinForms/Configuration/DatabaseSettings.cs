using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace Sicavi.WinForms.Configuration;

public sealed class DatabaseSettings
{
    public string Server { get; set; } = @".\WINCC";
    public string Database { get; set; } = "SICAVI";
    public bool IntegratedSecurity { get; set; } = true;
    public bool TrustServerCertificate { get; set; } = true;
    public int ConnectTimeoutSeconds { get; set; } = 8;

    public string BuildConnectionString(string? databaseOverride = null)
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = Server,
            InitialCatalog = databaseOverride ?? Database,
            IntegratedSecurity = IntegratedSecurity,
            TrustServerCertificate = TrustServerCertificate,
            Encrypt = true,
            ConnectTimeout = Math.Clamp(ConnectTimeoutSeconds, 3, 60),
            ApplicationName = "SICAVI"
        };

        return builder.ConnectionString;
    }

    public static DatabaseSettings LoadOrCreate(string path)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        if (!File.Exists(path))
        {
            var defaults = new DatabaseSettings();
            File.WriteAllText(path, JsonSerializer.Serialize(defaults, options));
            return defaults;
        }

        return JsonSerializer.Deserialize<DatabaseSettings>(File.ReadAllText(path), options)
            ?? throw new InvalidDataException("No se pudo leer database-settings.json.");
    }
}
