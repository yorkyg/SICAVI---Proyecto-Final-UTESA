using Microsoft.Data.SqlClient;
using Sicavi.WinForms.Configuration;

namespace Sicavi.WinForms.Data;

public sealed class SqlConnectionFactory
{
    private readonly DatabaseSettings _settings;

    public SqlConnectionFactory(DatabaseSettings settings)
    {
        _settings = settings;
    }

    public SqlConnection Create() => new(_settings.BuildConnectionString());
    public SqlConnection CreateMaster() => new(_settings.BuildConnectionString("master"));
}
