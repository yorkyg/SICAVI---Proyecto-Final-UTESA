namespace Sicavi.WinForms.Models;

public sealed record AppUser(
    int Id,
    string Username,
    string FullName,
    string Role,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastAccess)
{
    public bool IsAdministrator => Role.Equals("Administrador", StringComparison.OrdinalIgnoreCase);

    public static AppUser DesignUser { get; } = new(
        0,
        "diseno",
        "Usuario de diseño",
        "Administrador",
        true,
        DateTime.Now,
        null);
}
