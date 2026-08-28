using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Sicavi.WinForms.Security;

namespace Sicavi.WinForms.Data;

public sealed class DatabaseInitializer
{
    private readonly SqlConnectionFactory _connections;

    public DatabaseInitializer(SqlConnectionFactory connections)
    {
        _connections = connections;
    }

    public void Initialize(string scriptPath)
    {
        if (!File.Exists(scriptPath))
        {
            throw new FileNotFoundException("No se encontró el script de inicialización de SICAVI.", scriptPath);
        }

        string script = File.ReadAllText(scriptPath);
        string[] batches = Regex.Split(
            script,
            @"^\s*GO\s*(?:--.*)?$",
            RegexOptions.Multiline | RegexOptions.IgnoreCase);

        using SqlConnection connection = _connections.CreateMaster();
        connection.Open();

        foreach (string batch in batches.Where(value => !string.IsNullOrWhiteSpace(value)))
        {
            using SqlCommand command = connection.CreateCommand();
            command.CommandText = batch;
            command.CommandTimeout = 30;
            command.ExecuteNonQuery();
        }

        EnsureInitialAdministrator();
    }

    private void EnsureInitialAdministrator()
    {
        using SqlConnection connection = _connections.Create();
        connection.Open();

        using var countCommand = new SqlCommand("SELECT COUNT(*) FROM dbo.Usuarios;", connection);
        if (Convert.ToInt32(countCommand.ExecuteScalar()) > 0)
        {
            return;
        }

        (byte[] hash, byte[] salt) = PasswordHasher.Hash("Sicavi@123");
        using var insertCommand = new SqlCommand(
            """
            INSERT INTO dbo.Usuarios
                (NombreUsuario, NombreCompleto, ClaveHash, ClaveSalt, Rol, Activo)
            VALUES
                (@usuario, @nombre, @hash, @salt, N'Administrador', 1);
            """,
            connection);
        insertCommand.Parameters.AddWithValue("@usuario", "admin");
        insertCommand.Parameters.AddWithValue("@nombre", "Administrador SICAVI");
        insertCommand.Parameters.Add("@hash", System.Data.SqlDbType.VarBinary, 64).Value = hash;
        insertCommand.Parameters.Add("@salt", System.Data.SqlDbType.VarBinary, 32).Value = salt;
        insertCommand.ExecuteNonQuery();
    }
}
