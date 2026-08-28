using System.Data;
using Microsoft.Data.SqlClient;
using Sicavi.WinForms.Models;
using Sicavi.WinForms.Security;

namespace Sicavi.WinForms.Data;

public sealed class UserRepository
{
    private readonly SqlConnectionFactory _connections;

    public UserRepository(SqlConnectionFactory connections)
    {
        _connections = connections;
    }

    public AppUser? Authenticate(string username, string password)
    {
        string normalized = NormalizeUsername(username);
        using SqlConnection connection = _connections.Create();
        connection.Open();
        using var command = new SqlCommand(
            """
            SELECT Id, NombreUsuario, NombreCompleto, ClaveHash, ClaveSalt,
                   Rol, Activo, FechaCreacion, UltimoAcceso
            FROM dbo.Usuarios
            WHERE NombreUsuario = @usuario;
            """,
            connection);
        command.Parameters.Add("@usuario", SqlDbType.NVarChar, 50).Value = normalized;

        using SqlDataReader reader = command.ExecuteReader();
        if (!reader.Read() || !reader.GetBoolean(reader.GetOrdinal("Activo")))
        {
            return null;
        }

        byte[] hash = (byte[])reader["ClaveHash"];
        byte[] salt = (byte[])reader["ClaveSalt"];
        if (!PasswordHasher.Verify(password, hash, salt))
        {
            return null;
        }

        AppUser user = MapUser(reader);
        reader.Close();

        using var update = new SqlCommand(
            "UPDATE dbo.Usuarios SET UltimoAcceso = SYSDATETIME() WHERE Id = @id;",
            connection);
        update.Parameters.Add("@id", SqlDbType.Int).Value = user.Id;
        update.ExecuteNonQuery();
        return user with { LastAccess = DateTime.Now };
    }

    public IReadOnlyList<AppUser> GetAll()
    {
        var users = new List<AppUser>();
        using SqlConnection connection = _connections.Create();
        connection.Open();
        using var command = new SqlCommand(
            """
            SELECT Id, NombreUsuario, NombreCompleto, Rol, Activo, FechaCreacion, UltimoAcceso
            FROM dbo.Usuarios
            ORDER BY NombreUsuario;
            """,
            connection);
        using SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            users.Add(MapUser(reader));
        }
        return users;
    }

    public AppUser Create(
        string username,
        string fullName,
        string password,
        string role = "Operador",
        bool active = true)
    {
        string normalized = NormalizeUsername(username);
        string safeName = ValidateFullName(fullName);
        string safeRole = ValidateRole(role);
        (byte[] hash, byte[] salt) = PasswordHasher.Hash(password);

        using SqlConnection connection = _connections.Create();
        connection.Open();
        using var command = new SqlCommand(
            """
            INSERT INTO dbo.Usuarios
                (NombreUsuario, NombreCompleto, ClaveHash, ClaveSalt, Rol, Activo)
            OUTPUT INSERTED.Id, INSERTED.NombreUsuario, INSERTED.NombreCompleto,
                   INSERTED.Rol, INSERTED.Activo, INSERTED.FechaCreacion, INSERTED.UltimoAcceso
            VALUES
                (@usuario, @nombre, @hash, @salt, @rol, @activo);
            """,
            connection);
        command.Parameters.Add("@usuario", SqlDbType.NVarChar, 50).Value = normalized;
        command.Parameters.Add("@nombre", SqlDbType.NVarChar, 120).Value = safeName;
        command.Parameters.Add("@hash", SqlDbType.VarBinary, 64).Value = hash;
        command.Parameters.Add("@salt", SqlDbType.VarBinary, 32).Value = salt;
        command.Parameters.Add("@rol", SqlDbType.NVarChar, 20).Value = safeRole;
        command.Parameters.Add("@activo", SqlDbType.Bit).Value = active;

        try
        {
            using SqlDataReader reader = command.ExecuteReader();
            reader.Read();
            return MapUser(reader);
        }
        catch (SqlException exception) when (exception.Number is 2601 or 2627)
        {
            throw new InvalidOperationException("Ese nombre de usuario ya está registrado.", exception);
        }
    }

    public void Update(int id, string fullName, string role, bool active)
    {
        using SqlConnection connection = _connections.Create();
        connection.Open();
        using var command = new SqlCommand(
            """
            UPDATE dbo.Usuarios
            SET NombreCompleto = @nombre, Rol = @rol, Activo = @activo
            WHERE Id = @id;
            """,
            connection);
        command.Parameters.Add("@nombre", SqlDbType.NVarChar, 120).Value = ValidateFullName(fullName);
        command.Parameters.Add("@rol", SqlDbType.NVarChar, 20).Value = ValidateRole(role);
        command.Parameters.Add("@activo", SqlDbType.Bit).Value = active;
        command.Parameters.Add("@id", SqlDbType.Int).Value = id;
        if (command.ExecuteNonQuery() == 0)
        {
            throw new InvalidOperationException("El usuario seleccionado ya no existe.");
        }
    }

    public void ChangePassword(int id, string password)
    {
        (byte[] hash, byte[] salt) = PasswordHasher.Hash(password);
        using SqlConnection connection = _connections.Create();
        connection.Open();
        using var command = new SqlCommand(
            "UPDATE dbo.Usuarios SET ClaveHash = @hash, ClaveSalt = @salt WHERE Id = @id;",
            connection);
        command.Parameters.Add("@hash", SqlDbType.VarBinary, 64).Value = hash;
        command.Parameters.Add("@salt", SqlDbType.VarBinary, 32).Value = salt;
        command.Parameters.Add("@id", SqlDbType.Int).Value = id;
        command.ExecuteNonQuery();
    }

    private static AppUser MapUser(SqlDataReader reader) => new(
        reader.GetInt32(reader.GetOrdinal("Id")),
        reader.GetString(reader.GetOrdinal("NombreUsuario")),
        reader.GetString(reader.GetOrdinal("NombreCompleto")),
        reader.GetString(reader.GetOrdinal("Rol")),
        reader.GetBoolean(reader.GetOrdinal("Activo")),
        reader.GetDateTime(reader.GetOrdinal("FechaCreacion")),
        reader.IsDBNull(reader.GetOrdinal("UltimoAcceso"))
            ? null
            : reader.GetDateTime(reader.GetOrdinal("UltimoAcceso")));

    private static string NormalizeUsername(string value)
    {
        string normalized = value.Trim().ToLowerInvariant();
        if (normalized.Length is < 3 or > 50 || normalized.Any(ch => !(char.IsLetterOrDigit(ch) || ch is '.' or '_' or '-')))
        {
            throw new ArgumentException("El usuario debe tener entre 3 y 50 caracteres y usar letras, números, punto, guion o guion bajo.");
        }
        return normalized;
    }

    private static string ValidateFullName(string value)
    {
        string name = value.Trim();
        if (name.Length is < 3 or > 120)
        {
            throw new ArgumentException("El nombre completo debe tener entre 3 y 120 caracteres.");
        }
        return name;
    }

    private static string ValidateRole(string value) => value switch
    {
        "Administrador" => value,
        "Operador" => value,
        _ => throw new ArgumentException("Rol de usuario no válido.")
    };
}
